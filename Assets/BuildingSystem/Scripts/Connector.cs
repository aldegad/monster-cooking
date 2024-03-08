using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public ConnectorPosition connectorPosition;
    public SelectedBuildType connectorParentType;

    [HideInInspector] public bool isConnectedToFloor = false;
    [HideInInspector] public bool isConnectedToWall = false;
    [HideInInspector] public bool canConnectedTo = true;

    [SerializeField] private bool canConnectToFloor = true;
    [SerializeField] private bool canConnectToWall = true;

    private void OnDrawGizmos()
    {
        if (isConnectedToFloor && isConnectedToWall)
        {
            Gizmos.color = Color.green;
        }

        if (isConnectedToFloor)
        {
            Gizmos.color = Color.yellow;
        }

        if (isConnectedToWall)
        {
            Gizmos.color = Color.blue;
        }

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x / 2f);
    }

    public void updateConnectors(bool rootCall = false)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.lossyScale.x / 2f);

        isConnectedToFloor = !canConnectToFloor;
        isConnectedToWall = !canConnectToWall;

        foreach (Collider collider in colliders)
        {
            if (collider.GetInstanceID() == GetComponent<Collider>().GetInstanceID())
            {
                continue;
            }

            if (collider.gameObject.layer == gameObject.layer)
            { 
                Connector foundConnector = collider.GetComponent<Connector>();

                if (foundConnector.connectorParentType == SelectedBuildType.floor)
                {
                    isConnectedToFloor = true;
                }

                if (foundConnector.connectorParentType == SelectedBuildType.wall)
                {
                    isConnectedToWall = true;
                }

                if (rootCall)
                { 
                    foundConnector.updateConnectors();
                }
            }
        }

        canConnectedTo = true;

        if (isConnectedToFloor && isConnectedToWall)
        {
            canConnectedTo = false;
        }
    }
}

[Serializable]
public enum ConnectorPosition
{
    left,
    right,
    top,
    bottom
}