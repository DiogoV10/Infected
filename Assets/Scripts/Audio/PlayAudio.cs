using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    public string audioClipName;
    public void PlaySound()
    {
        FindObjectOfType<AudioManager>().PlaySound(audioClipName);
    }
}
