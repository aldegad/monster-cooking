using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableModule : MonoBehaviour
{
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
