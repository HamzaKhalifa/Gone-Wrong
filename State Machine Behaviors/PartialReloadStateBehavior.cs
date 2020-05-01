using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialReloadStateBehavior : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        GoneWrong.WeaponControl weaponControl = animator.GetComponent<GoneWrong.WeaponControl>();
        SharedInt equippedWeapon = weaponControl.equippedWeapon;
        Inventory playerInventory = weaponControl.playerInventory;

        // Get the weapon mount of the current equipped weapon
        InventoryWeaponMount weaponMount = null;
        if (equippedWeapon.value == 0) weaponMount = playerInventory.rifle1;
        else if (equippedWeapon.value == 1) weaponMount = playerInventory.rifle2;
        else if (equippedWeapon.value == 2) weaponMount = playerInventory.handgun;
        if (equippedWeapon.value == 3) weaponMount = playerInventory.melee;


        // We need to keep track of weather we found rounds in our invventory if the weapon is partial
        // So we could stop the reload animation
        bool foundRounds = false;

        for (int i = 0; i < playerInventory.ammo.Count; i++)
        {
            // We get the ammo mount for each ammo we have
            InventoryAmmoMount ammoMount = playerInventory.ammo[i];
            if (ammoMount == null || ammoMount.item == null) continue;

            // We check if the ammo applies to the equipped weapon
            if (ammoMount.item.weapon == weaponMount.item)
            {
                // If the weapon reload type is partial, we just add one single bullet
                if (weaponMount.item.partialReload)
                {
                    // If we have attained the last round that can be loaded, then we force ourselves to stop reloading
                    if (weaponMount.rounds >= weaponMount.item.ammoCapacity)
                    {
                        animator.SetBool("Reload", false);
                    }

                    foundRounds = true;

                    break;
                }
            }
        }

        if (weaponMount.item.partialReload && !foundRounds)
        {
            animator.SetBool("Reload", false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
