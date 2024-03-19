using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "buildable object", menuName = "Building System/buildable object")]
public class BuildableObject : ScriptableObject
{
    [SerializeField] public string displayName;
    [SerializeField] public string description;
    [SerializeField] public Texture2D thumbnailImage;
    [SerializeField] public BuildType buildType;
    [SerializeField] public GameObject buildableObject;
}
