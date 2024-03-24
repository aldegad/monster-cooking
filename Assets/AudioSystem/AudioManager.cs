using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioLayer> audioLayers = new List<AudioLayer>();
}


[Serializable]
public class AudioLayer
{
    public string layerName;
    public AudioClip sound;
    [Range(0f, 1f)]
    public float volume;
}