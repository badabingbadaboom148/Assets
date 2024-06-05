using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioScript : MonoBehaviour
{
    [SerializeField] private EventReference DialogueSFX;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class FmodEvents
{
    public static void PlayOneShot(EventReference Sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(Sound, worldPosition);
    }

    public static EventInstance CreateEventInstance(EventReference eventReference)
    {
        return RuntimeManager.CreateInstance(eventReference);
    }
}