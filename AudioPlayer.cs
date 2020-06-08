using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private List<AudioClip> clips = new List<AudioClip>();

    public void PlayAudio(int index)
    {
        if (clips.Count > index && GoneWrong.AudioManager.instance != null && clips[index] != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(clips[index], 1, 0, 0);
        }
    }

    public void PlayRandom()
    {
        if (clips.Count == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Count)];
        if (clip != null)
        {
            GoneWrong.AudioManager.instance.PlayOneShotSound(clip, 1, 0, 0);
        }
    }
}
