using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public ConnectorPosition connectorPosition;
    public BuildType connectorParentType;
    public bool isGhostParent = false;

    [SerializeField] public bool isConnectedToFloor = false;
    [SerializeField] public bool isConnectedToWall = false;
    [SerializeField] public bool canConnectedTo = true;

    [SerializeField] private bool canConnectToFloor = true;
    [SerializeField] private bool canConnectToWall = true;

    private void OnDrawGizmos()
    {
        if (!isConnectedToFloor && !isConnectedToWall)
        {
            Gizmos.color = Color.green;
        }

        else if (!isConnectedToFloor)
        {
            Gizmos.color = Color.yellow;
        }

        else if (!isConnectedToWall)
        {
            Gizmos.color = Color.blue;
        }
        else
        {
            Gizmos.color = Color.red;
        }

        Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x / 2.5f);
    }

    public void updateConnectors(bool rootCall = false)
    {
        Collider myCollider = gameObject.GetComponent<Collider>();

        LayerMask maskFromLayer = 1 << gameObject.layer; // 레이어 인덱스를 이용해 LayerMask 생성
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.lossyScale.x / 2f, maskFromLayer);

        isConnectedToFloor = !canConnectToFloor;
        isConnectedToWall = !canConnectToWall;

        foreach (Collider collider in colliders)
        {
            if (collider == myCollider) { continue; }

            Connector foundConnector = collider.GetComponent<Connector>();

            if (foundConnector == null) { continue; }
            if (foundConnector.isGhostParent) { continue; }

            if (foundConnector.connectorParentType == BuildType.floor)
            {
                isConnectedToFloor = true;
            }

            if (foundConnector.connectorParentType == BuildType.wall)
            {
                isConnectedToWall = true;
            }

            if (rootCall)
            {
                foundConnector.updateConnectors();
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