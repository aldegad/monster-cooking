using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "buildable object", menuName = "Building System/buildable object")]
public class BuildingModule : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Texture2D thumbnailImage;
    [SerializeField] private BuildType buildType;
    [SerializeField] private GameObject buildableObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
