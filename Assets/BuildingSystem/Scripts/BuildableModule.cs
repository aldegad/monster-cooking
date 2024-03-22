using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BuildableModule : NetworkBehaviour
{
    [Header("Info")]
    [SerializeField] public string displayName;
    [SerializeField] public string description;
    [SerializeField] public Texture2D thumbnailImage;
    [SerializeField] public BuildType buildType;

    [Header("Ghost Setting")]
    [SerializeField] private GameObject modelParent;
    [SerializeField] private GameObject connectorParent;
    [SerializeField] private bool _isGhost = false;

    public GameObject ModelParent => modelParent;
    public GameObject ConnectorParent => connectorParent;
    public bool isGhost
    { 
        get { return _isGhost; }
        set { 
            _isGhost = value;
            Connector[] connectors = connectorParent.GetComponentsInChildren<Connector>();
            foreach (Connector connector in connectors)
            {
                connector.isGhostParent = value;
            }
        }
    }
}
