using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DataType { None, Weapon, Ammo, Consumable, Message }
public enum DayTime { Night, Mist }

[System.Serializable]
public class SavedSingleData 
{
    public DataType type = DataType.None;
    public int index = -1;
    public int rounds = -1;
}

[System.Serializable]
public class SceneCollectableItem
{
    public DataType type = DataType.None;
    public int index = -1;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public string parentName = "";
}

[System.Serializable]
public class ProgressObject
{
    public int index = -1;
    public bool active = true;
    public Vector3 position = Vector3.zero;
}

[System.Serializable]
public class SceneData
{
    public bool savedBefore = false;
    public List<SceneCollectableItem> collectableItems = new List<SceneCollectableItem>();
    public List<string> remainingZombies = new List<string>();
    public List<ProgressObject> progressObjects = new List<ProgressObject>();
    public string nextObjective = "";
    public DayTime dayTime = DayTime.Night;
}


[System.Serializable]
public class SavedData
{
    public int currentScene = 0;

    public SavedSingleData rifle1 = new SavedSingleData();
    public SavedSingleData rifle2 = new SavedSingleData();
    public SavedSingleData handgun = new SavedSingleData();
    public SavedSingleData melee = new SavedSingleData();

    public List<SavedSingleData> inventoryList = new List<SavedSingleData>();

    public float health = 100;
    public float stamina = 100;
    public float infection = 0;

    public Vector3 playerPosition = new Vector3();
    public Quaternion playerRotation = new Quaternion();

    public SceneData hospitalSceneData = new SceneData();
    public SceneData citySceneData = new SceneData();
    public SceneData wasteLandData = new SceneData();

    public List<SceneCollectableItem> hospitalCollectableItems = new List<SceneCollectableItem>();
}

public class SaveGame : InteractiveObject
{
    public static SaveGame instance = null;

    [SerializeField] Material _fogSkyBox = null;
    [SerializeField] private bool _deleteSaveData = false;
    [SerializeField] private bool _saveZombies = true;

    // Inspector
    [SerializeField] Inventory _playerInventory = null;
    [SerializeField] SharedFloat _healthSharedFloat = null;
    [SerializeField] SharedFloat _staminaSharedFloat = null;
    [SerializeField] SharedFloat _infectionSharedFloat = null;

    [SerializeField] List<InventoryWeapon> _inventoryWeapons = new List<InventoryWeapon>();
    [SerializeField] List<InventoryAmmo> _inventoryAmmos = new List<InventoryAmmo>();
    [SerializeField] List<InventoryConsumable> _inventoryConsumables = new List<InventoryConsumable>();
    [SerializeField] List<Messages> _messages = new List<Messages>();
    [SerializeField] List<Transform> _progressObjects = new List<Transform>();

    [SerializeField] private Material _onMaterial = null;
    [SerializeField] private Material _offMaterial = null;

    [SerializeField] AudioClip _onSound = null;
    [SerializeField] AudioClip _offSound = null;

    [SerializeField] private string _saveText = "Are you sure you want to save? CLick \"E\" again to save";

    // Cash Variables
    private MeshRenderer _meshRenderer = null;

    // Private
    private bool _on = false;
    private IEnumerator _automaticShutDownCoroutine = null;
    private SavedData _savedData = null;

    public SavedData savedData { get { return _savedData; } }

    public void DeleteSaveGame()
    {
        PlayerPrefs.DeleteAll();
    }

    public SavedData GetSavedData()
    {
        SavedData savedData = JsonUtility.FromJson<SavedData>(PlayerPrefs.GetString("data"));
        return savedData;
    }

    private void Awake()
    {
        if (SaveGame.instance != null)
        {
            return;
        }

        instance = this;

        if (_deleteSaveData) {
            DeleteSaveGame();
            return;
        }

        SavedData savedData = JsonUtility.FromJson<SavedData>(PlayerPrefs.GetString("data"));
        _savedData = savedData;

        List<InventoryAmmoMount> ammos = new List<InventoryAmmoMount>();
        List<InventoryConsumableMount> consumables = new List<InventoryConsumableMount>();
        List<Messages> messages = new List<Messages>();

        if (_playerInventory != null && savedData != null)
        {
            if (savedData.rifle1.index != -1)
            {
                _playerInventory.rifle1.item = _inventoryWeapons[savedData.rifle1.index];
                _playerInventory.rifle1.rounds = savedData.rifle1.rounds;
            }

            if (savedData.rifle2.index != -1) {
                _playerInventory.rifle2.item = _inventoryWeapons[savedData.rifle2.index];
                _playerInventory.rifle2.rounds = savedData.rifle2.rounds;
            }

            if (savedData.handgun.index != -1)
            {
                _playerInventory.handgun.item = _inventoryWeapons[savedData.handgun.index];
                _playerInventory.handgun.rounds = savedData.handgun.rounds;
            }

            if (savedData.melee.index != -1)
            {
                _playerInventory.melee.item = _inventoryWeapons[savedData.melee.index];
            }

            foreach (SavedSingleData savedSingleData in savedData.inventoryList)
            {
                switch (savedSingleData.type)
                {
                    case DataType.Ammo:
                        if (_inventoryAmmos.Count > savedSingleData.index)
                        {
                            InventoryAmmoMount ammoMount = new InventoryAmmoMount();
                            ammoMount.rounds = savedSingleData.rounds;
                            ammoMount.item = _inventoryAmmos[savedSingleData.index];
                            ammos.Add(ammoMount);
                        }
                        break;
                    case DataType.Consumable:
                        if (_inventoryConsumables.Count > savedSingleData.index)
                        {
                            InventoryConsumableMount consumableMount = new InventoryConsumableMount();
                            consumableMount.item = _inventoryConsumables[savedSingleData.index];

                            // Now we alter the game progress
                            // Before adding each consumable, we need to make sure that the progress states associated with the mare registered
                            List<GameProgress> progressStates = consumableMount.item.collectableConsumable.progressStates;
                            ProgressManager progressManager = FindObjectOfType<ProgressManager>();
                            if (progressStates.Count > 0 && progressManager != null)
                            {
                                foreach (GameProgress gameProgress in progressStates)
                                {
                                    progressManager.SetProgress(gameProgress.key, gameProgress.value);
                                }
                            }

                            consumables.Add(consumableMount);
                        }
                        break;
                    case DataType.Message:
                        if (_messages.Count > savedSingleData.index)
                        {
                            Messages message = _messages[savedSingleData.index];
                            messages.Add(message);
                        }
                        break;
                }

                _playerInventory.ammo = ammos;
                _playerInventory.consumables = consumables;
                _playerInventory.messages = messages;
            }

            if (_healthSharedFloat != null)
                _healthSharedFloat.value = savedData.health;

            if (_staminaSharedFloat != null)
                _staminaSharedFloat.value = savedData.stamina;

            if (_infectionSharedFloat != null)
                _infectionSharedFloat.value = savedData.infection;

            // We only load the player position if the last saved scene is the same as the current scene
            // So that we dont load the player in a random position when he just entered the map from another map
            GoneWrong.Player player = FindObjectOfType<GoneWrong.Player>();
            if (player != null && _savedData.currentScene == SceneManager.GetActiveScene().buildIndex)
            {
                CharacterController playerCharacterController = player.GetComponent<CharacterController>();
                playerCharacterController.enabled = false;

                if (savedData.playerPosition != Vector3.zero)
                {
                    player.transform.position = savedData.playerPosition;
                }

                if (savedData.playerRotation != Quaternion.identity)
                {
                    player.transform.rotation = savedData.playerRotation;
                }

                playerCharacterController.enabled = true;
            }

            // Now for the collectable items
            // We check if we already saved the data for the current scene:
            SceneData sceneData = null;
            switch(SceneManager.GetActiveScene().name)
            {
                case "Hospital":
                    sceneData = savedData.hospitalSceneData;
                    break;
                case "City":
                    sceneData = savedData.citySceneData;
                    break;
                case "Wasteland":
                    sceneData = savedData.wasteLandData;
                    break;
            }

            if (sceneData != null && sceneData.savedBefore)
            {
                // We change the skybox and the fog depending on what is stored in the sceneData
                switch(sceneData.dayTime)
                {
                    case DayTime.Night:
                        RenderSettings.fogDensity = 0f;
                        RenderSettings.skybox = null;
                        break;
                    case DayTime.Mist:
                        if (_fogSkyBox !=null)
                        {
                            RenderSettings.fogDensity = 0.35f;
                            RenderSettings.skybox = _fogSkyBox;
                        }
                        break;
                }

                // We already saved the scene before. So we destroy all the collectableItems in the scene
                CollectableConsumable[] collectableConsumables = FindObjectsOfType<CollectableConsumable>();
                foreach(CollectableConsumable collectableConsumable in collectableConsumables)
                {
                    if (collectableConsumable.canSave)
                        Destroy(collectableConsumable.gameObject);
                }

                CollectableAmmo[] colectableAmmos = FindObjectsOfType<CollectableAmmo>();
                foreach (CollectableAmmo collectableAmmo in colectableAmmos)
                {
                    Destroy(collectableAmmo.gameObject);
                }

                // Then we instantiate all the remaining collectable Items that were present when saving the data
                foreach (SceneCollectableItem sceneCollectableItem in sceneData.collectableItems)
                {
                    // If we have the collectable item in list of collectable items
                    if (sceneCollectableItem.index != -1)
                    {
                        // Then we instantiate the collectable item from the inventory scriptable object in our list
                        if (sceneCollectableItem.type == DataType.Consumable)
                        {
                            if (_inventoryConsumables[sceneCollectableItem.index].collectableConsumable != null)
                            {
                                CollectableConsumable tmp = Instantiate(_inventoryConsumables[sceneCollectableItem.index].collectableConsumable);
                                if (sceneCollectableItem.parentName != "")
                                {
                                    Transform parent = GameObject.Find(sceneCollectableItem.parentName).transform;
                                    if (parent != null)
                                        tmp.transform.parent = parent;

                                    tmp.transform.localPosition = sceneCollectableItem.localPosition;
                                    tmp.transform.localRotation = sceneCollectableItem.localRotation;
                                }
                            }
                        } else if (sceneCollectableItem.type == DataType.Ammo)
                        {
                            if (_inventoryConsumables[sceneCollectableItem.index].collectableConsumable != null)
                            {
                                CollectableAmmo tmp = Instantiate(_inventoryAmmos[sceneCollectableItem.index].collectableAmmo);
                                if (sceneCollectableItem.parentName != "")
                                {
                                    Transform parent = GameObject.Find(sceneCollectableItem.parentName).transform;
                                    if (parent != null)
                                        tmp.transform.parent = parent;

                                    tmp.transform.localPosition = sceneCollectableItem.localPosition;
                                    tmp.transform.localRotation = sceneCollectableItem.localRotation;
                                }
                            }
                        }
                    }
                }

                // Now we check the remaining zombies
                // Each zombie whose name doesn't belong to the list of the remaining zombies, will be deactivated
                if (_saveZombies)
                {
                    AIStateMachine[] stateMachines = FindObjectsOfType<AIStateMachine>();
                    foreach (AIStateMachine stateMachine in stateMachines)
                    {
                        int index = 0;
                        bool foundZombie = false;
                        do
                        {
                            if (sceneData.remainingZombies.Count > index && stateMachine.transform.name == sceneData.remainingZombies[index])
                            {
                                foundZombie = true;
                            }
                            index++;
                        } while (index < sceneData.remainingZombies.Count && !foundZombie);

                        // If we didn't find the zombie in our list, we destroy him
                        if (!foundZombie)
                        {
                            Destroy(stateMachine.gameObject);
                            //stateMachine.gameObject.SetActive(false);
                        }
                    }
                }

                // Handling progress objects (activating and deactivating them, then setting their position)
                foreach (ProgressObject progressObject in sceneData.progressObjects)
                {
                    if (progressObject.index != -1 && _progressObjects.Count > progressObject.index)
                    {
                        Transform sceneProgressObject = _progressObjects[progressObject.index];
                        if (sceneProgressObject != null)
                        {
                            sceneProgressObject.gameObject.SetActive(progressObject.active);
                            sceneProgressObject.transform.position = progressObject.position;
                        }
                    }
                }

                PlayerHUD playerHud = PlayerHUD.instance;
                if (playerHud == null) playerHud = FindObjectOfType<PlayerHUD>();

                if (playerHud != null)
                {
                    playerHud.ChangeLevelObjectiveText(sceneData.nextObjective);
                }
            }
        }
    }

    protected override void Start()
    {
        base.Start();

        _meshRenderer = GetComponent<MeshRenderer>();

        if (_meshRenderer != null)
        {
            _meshRenderer.material = _on ? _onMaterial : _offMaterial;
        }
    }

    public void Save()
    {
        SavedData savedData = new SavedData();

        savedData.currentScene = SceneManager.GetActiveScene().buildIndex;

        // We get the corresponding scene items first
        SceneData sceneData = null;
        switch (SceneManager.GetActiveScene().name)
        {
            case "Hospital":
                sceneData = savedData.hospitalSceneData;
                break;
            case "Wasteland":
                sceneData = savedData.wasteLandData;
                break;
        }

        if (_playerInventory != null)
        {
            List<InventoryWeaponMount> weaponMounts = new List<InventoryWeaponMount>();
            weaponMounts.Add(_playerInventory.rifle1);
            weaponMounts.Add(_playerInventory.rifle2);
            weaponMounts.Add(_playerInventory.handgun);
            weaponMounts.Add(_playerInventory.melee);
            for (int i = 0; i < weaponMounts.Count; i++)
            {
                if (weaponMounts[i].item == null) continue;

                bool foundWeapon = false;
                int index = 0;
                do
                {
                    if (weaponMounts[i].item == _inventoryWeapons[index])
                    {
                        foundWeapon = true;
                        SavedSingleData savedSingleData = new SavedSingleData();
                        savedSingleData.type = DataType.Weapon;
                        savedSingleData.index = index;
                        savedSingleData.rounds = weaponMounts[i].rounds;
                        if (i == 0)
                            savedData.rifle1 = savedSingleData;
                        else if (i == 1)
                            savedData.rifle2 = savedSingleData;
                        else if (i == 2)
                            savedData.handgun = savedSingleData;
                        else if (i == 3)
                            savedData.melee = savedSingleData;
                    }

                    index++;

                } while (!foundWeapon && index < _inventoryWeapons.Count);
            }

            foreach (InventoryAmmoMount ammoMount in _playerInventory.ammo)
            {
                bool foundAmmo = false;
                int index = 0;
                do
                {
                    if (ammoMount.item == _inventoryAmmos[index])
                    {
                        foundAmmo = true;
                        SavedSingleData savedSingleData = new SavedSingleData();
                        savedSingleData.type = DataType.Ammo;
                        savedSingleData.index = index;
                        savedSingleData.rounds = ammoMount.rounds;
                        savedData.inventoryList.Add(savedSingleData);
                    }

                    index++;

                } while (!foundAmmo && index < _inventoryAmmos.Count);
            }

            foreach (InventoryConsumableMount consumableMount in _playerInventory.consumables)
            {
                bool foundConsumable = false;
                int index = 0;
                do
                {
                    if (consumableMount.item == _inventoryConsumables[index])
                    {
                        foundConsumable = true;
                        SavedSingleData savedSingleData = new SavedSingleData();
                        savedSingleData.type = DataType.Consumable;
                        savedSingleData.index = index;
                        savedData.inventoryList.Add(savedSingleData);
                    }

                    index++;

                } while (!foundConsumable && index < _inventoryConsumables.Count);
            }

            foreach(Messages message in _playerInventory.messages)
            {
                bool foundMessage = false;
                int index = 0;
                do
                {
                    if (message == _messages[index])
                    {
                        foundMessage = true;
                        SavedSingleData savedSingleData = new SavedSingleData();
                        savedSingleData.type = DataType.Message;
                        savedSingleData.index = index;
                        savedData.inventoryList.Add(savedSingleData);
                    }

                    index++;

                } while (!foundMessage && index < _messages.Count);
            }

            if (_healthSharedFloat != null)
                savedData.health = _healthSharedFloat.value;

            if (_staminaSharedFloat != null)
                savedData.stamina = _staminaSharedFloat.value;

            if (_infectionSharedFloat != null)
                savedData.infection = _infectionSharedFloat.value;
        }

        // Now we are about to save the scene items:
        if (sceneData != null && sceneData.collectableItems != null)
        {
            // Now saving the night sky and the fog
            sceneData.dayTime = RenderSettings.fog ? DayTime.Mist : DayTime.Night;

            CollectableConsumable[] consumables = FindObjectsOfType<CollectableConsumable>();
            // We save the remaining collectable consumables
            foreach (CollectableConsumable consumable in consumables)
            {
                // Some key items are set to never be saved. They are managed by progress states
                if (!consumable.canSave) continue;

                SceneCollectableItem sceneCollectableItem = new SceneCollectableItem();
                sceneCollectableItem.type = DataType.Consumable;
                sceneCollectableItem.localPosition = consumable.transform.localPosition;
                sceneCollectableItem.localRotation = consumable.transform.localRotation;
                if (consumable.transform.parent != null)
                    sceneCollectableItem.parentName = consumable.transform.parent.name;

                bool foundConsumable = false;
                int index = 0;
                do
                {
                    if (consumable.consumableMount.item == _inventoryConsumables[index])
                    {
                        foundConsumable = true;
                        sceneCollectableItem.index = index;
                    }

                    index++;

                } while (!foundConsumable && index < _inventoryConsumables.Count);

                // Now that we got our scene collectable item data stored in an object, we add them to the corresponding scene
                sceneData.collectableItems.Add(sceneCollectableItem);
            }

            // Now we save the remaining collectable ammos:
            CollectableAmmo[] ammos = FindObjectsOfType<CollectableAmmo>();
            foreach (CollectableAmmo ammo in ammos)
            {
                SceneCollectableItem sceneCollectableItem = new SceneCollectableItem();
                sceneCollectableItem.type = DataType.Ammo;
                sceneCollectableItem.localPosition = ammo.transform.localPosition;
                sceneCollectableItem.localRotation = ammo.transform.localRotation;
                if (ammo.transform.parent != null)
                    sceneCollectableItem.parentName = ammo.transform.parent.name;

                bool foundAmmo = false;
                int index = 0;
                do
                {
                    if (ammo.ammoMount.item == _inventoryAmmos[index])
                    {
                        foundAmmo = true;
                        sceneCollectableItem.index = index;
                    }

                    index++;

                } while (!foundAmmo && index < _inventoryAmmos.Count);

                // Now that we got our scene collectable item data stored in an object, we add them to the corresponding scene
                sceneData.collectableItems.Add(sceneCollectableItem);
            }

            // Now we check for the remaining zombies:
            if (_saveZombies)
            {
                AIStateMachine[] stateMachines = FindObjectsOfType<AIStateMachine>();
                foreach (AIStateMachine stateMachine in stateMachines)
                {
                    // If the zombie is activated and he is not dead, then we save him
                    if (stateMachine.gameObject.activeSelf && !stateMachine.dead)
                    {
                        sceneData.remainingZombies.Add(stateMachine.transform.name);
                    }
                }
            }

            // Now saving progress objects for in sceneData
            for (int i = 0; i < _progressObjects.Count; i++)
            {
                ProgressObject progressObject = new ProgressObject();
                progressObject.index = i;
                if (_progressObjects[i] != null)
                {
                    progressObject.active = _progressObjects[i].gameObject.activeSelf;
                    progressObject.position = _progressObjects[i].transform.position;
                }
                else
                {
                    progressObject.active = false;
                }

                sceneData.progressObjects.Add(progressObject);
            }

            // Now saving the next level objective text
            if (PlayerHUD.instance != null)
            {
                sceneData.nextObjective = PlayerHUD.instance.nextObjective;
            }

            // Then we set the scene saved state to true
            sceneData.savedBefore = true;
        }


        // Now we save the player position
        GoneWrong.Player player = GoneWrong.Player.instance;
        if (player != null)
        {
            savedData.playerPosition = player.transform.position;
            savedData.playerRotation = player.transform.rotation;
        }

        // Now we save in player prefs:
        string data = JsonUtility.ToJson(savedData);
        
        PlayerPrefs.SetString("data", data);
    }

    public override void Interact(Transform interactor)
    {
        base.Interact(interactor);

        if (_meshRenderer != null && _onMaterial != null && _offMaterial != null)
        {
            _on = !_on;
            _meshRenderer.material = _on ? _onMaterial : _offMaterial;

            if (GoneWrong.AudioManager.instance != null)
            {
                AudioClip clip = _on ? _onSound : _offSound;
                if (clip != null)
                {
                    GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 0);
                }
            }
        }

        _text = _on ? _saveText : _interactiveText;

        if (!_on)
        {
            if (Notifications.instance != null)
            {
                Save();
                Notifications.instance.EnqueNotification("Progress Saved!");
            }
        } else
        {
            _automaticShutDownCoroutine = AutomaticShutdown();
            StartCoroutine(_automaticShutDownCoroutine);
        }
    }

    public IEnumerator AutomaticShutdown()
    {
        yield return new WaitForSeconds(10);

        if (_on)
        {
            if (_meshRenderer != null)
                _meshRenderer.material = _offMaterial;

            _text = _interactiveText;

            if (GoneWrong.AudioManager.instance != null)
            {
                if (_offSound != null)
                {
                    GoneWrong.AudioManager.instance.PlayOneShotSound(_offSound, 1, 0, 0);
                }
            }

            _on = !_on;
        }

        _automaticShutDownCoroutine = null;
    }
}
