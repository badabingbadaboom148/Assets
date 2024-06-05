using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SFXManager : MonoBehaviour
{
    public AudioClip[] voicelines;
    private AudioSource audioSource;
    public string[] shipNames;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayRandomVoiceline()
    {
        int index = Random.Range(0, voicelines.Length);
        AudioClip voiceline = voicelines[index];
        if (audioSource != null && voiceline != null)
        {
            // Set the AudioClip to play
            audioSource.clip = voiceline;

            // Play the audio clip
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource or sound effect clip is not set.");
        }
    }
    public string SelectRandomShipName()
    {
        int index = Random.Range(0, shipNames.Length);
        return shipNames[index];
    }
}
