using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioLayer> audioLayers = new List<AudioLayer>();
    [SerializeField] private AudioLayer sampleAudio;

    private void Update()
    {
        
    }
}


[Serializable]
public class AudioLayer
{
    public string layerName;
    public AudioClip sound;
    [Range(0f, 1f)]
    public float volume = 1f;
    public string description;
}