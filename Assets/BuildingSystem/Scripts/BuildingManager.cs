using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Cinemachine;

public class BuildingManager : MonoBehaviour
{
    [Header("Build Objects")]
    [SerializeField] private List<GameObject> floorObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> wallObjects = new List<GameObject>();

    [Header("Build Settings")]
    [SerializeField] private SelectedBuildType currentBuildType;
    [SerializeField] private LayerMask connectorLayer;

    [Header("Destroy Settings")]
    [SerializeField] private bool isDestroying = false;
    private Transform lastHitDestroyTransform;
    private List<Material> LastHitMaterials = new List<Material>();


    [Header("Ghost Settings")]
    [SerializeField] private Material ghostMaterialValid;
    [SerializeField] private Material ghostMaterialInvalid;
    [SerializeField] private float connectorOverlapRadius = 1f;
    [SerializeField] private float maxGroundAngle = 45f;

    [Header("Internal State")]
    [SerializeField] private bool isBuilding = false;
    [SerializeField] private int currentBuildingIndex;

    private GameObject ghostBuildGameObject;
    private bool isGhostInvalidPosition = false;
    private Transform ModelParent = null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuilding = !isBuilding;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            isDestroying = !isDestroying;
        }

        if (isBuilding && !isDestroying)
        {
            ghostBuild();

            if (Input.GetMouseButtonDown(0))
            {
                placeBuild();
            }
        }
        else if (ghostBuildGameObject)
        { 
            Destroy(ghostBuildGameObject);
            ghostBuildGameObject = null;
        }

        if (isDestroying)
        {
            ghostDestroy();

            if (Input.GetMouseButtonDown(0))
            {
                destroyBuild();
            }
        }
    }

    private void ghostBuild()
    {
        GameObject currentBuild = getCurrentBuild();
        createGhostPrefab(currentBuild);

        moveGhostPrefabToRaycast();
        checkBuildValidity();
    }

    private void createGhostPrefab(GameObject currentBuild)
    {
        if (ghostBuildGameObject == null)
        { 
            ghostBuildGameObject = Instantiate(currentBuild);

            ModelParent = ghostBuildGameObject.transform.GetChild(0);

            ghostifyModel(ModelParent, ghostMaterialValid);
            ghostifyModel(ghostBuildGameObject.transform);
        }
    }

    private void moveGhostPrefabToRaycast()
    { 
        // 화면 중앙에서 레이를 생성합니다.
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // 레이캐스트가 어떤 오브젝트에 맞았다면, 'ghostBuildGameObject'의 위치를 맞은 지점으로 이동시킵니다.
            ghostBuildGameObject.transform.position = hit.point;
        }
    }

    private void checkBuildValidity()
    {
        Collider[] colliders = Physics.OverlapSphere(ghostBuildGameObject.transform.position, connectorOverlapRadius, connectorLayer);

        if (colliders.Length > 0)
        {
            ghostConnectBuild(colliders);
        }
        else
        {
            ghostSeperateBuild();

            if (isGhostInvalidPosition)
            {
                Collider[] overlapColliders = Physics.OverlapBox(ghostBuildGameObject.transform.position, new Vector3(2f, 2f, 2f), ghostBuildGameObject.transform.rotation);
                foreach (Collider overapCollider in overlapColliders)
                {
                    if (overapCollider.gameObject != ghostBuildGameObject && overapCollider.transform.root.CompareTag("Buildables"))
                    {
                        ghostifyModel(ModelParent, ghostMaterialInvalid);
                        isGhostInvalidPosition = false;
                        return;
                    }
                }
            }
        }
    }

    private void ghostConnectBuild(Collider[] colliders)
    {
        Connector bestConnector = null;

        foreach (Collider collider in colliders)
        { 
            Connector connector = collider.GetComponent<Connector>();

            if (connector.canConnectedTo)
            {
                bestConnector = connector;
                break;
            }
        }

        if (bestConnector == null ||
            currentBuildType == SelectedBuildType.floor && bestConnector.isConnectedToFloor ||
            currentBuildType == SelectedBuildType.wall && bestConnector.isConnectedToWall)
        {
            ghostifyModel(ModelParent, ghostMaterialInvalid);
            isGhostInvalidPosition = false;
            return;
        }

        snapGhostPrefabToConnector(bestConnector);
    }

    private void snapGhostPrefabToConnector(Connector connector)
    {
        Transform ghostConnector = findSnapConnector(connector.transform, ghostBuildGameObject.transform.GetChild(1));
        ghostBuildGameObject.transform.position = connector.transform.position - (ghostConnector.position - ghostBuildGameObject.transform.position);

        if (currentBuildType == SelectedBuildType.wall)
        {
            Quaternion newRotation = ghostBuildGameObject.transform.rotation;
            newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.rotation.eulerAngles.y, newRotation.eulerAngles.z);
            ghostBuildGameObject.transform.rotation = newRotation;
        }

        ghostifyModel(ModelParent, ghostMaterialValid);
        isGhostInvalidPosition = true;
    }

    private void ghostSeperateBuild()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if(currentBuildType == SelectedBuildType.wall)
            {
                ghostifyModel(ModelParent, ghostMaterialInvalid);
                isGhostInvalidPosition = false;
                return;
            }

            if (Vector3.Angle(hit.normal, Vector3.up) < maxGroundAngle)
            {
                ghostifyModel(ModelParent, ghostMaterialValid);
                isGhostInvalidPosition = true;
            }
            else
            {
                ghostifyModel(ModelParent, ghostMaterialInvalid);
                isGhostInvalidPosition = false;
            }
        }
    }

    private Transform findSnapConnector(Transform snapConnector, Transform ghostConnectorParent)
    {
        ConnectorPosition OppositeConnectorTag = getOppositePosition(snapConnector.GetComponent<Connector>());

        foreach (Connector connector in ghostConnectorParent.GetComponentsInChildren<Connector>())
        {
            if (connector.connectorPosition == OppositeConnectorTag)
            {
                return connector.transform;
            }
        }

        return null;
    }

    private ConnectorPosition getOppositePosition(Connector connector)
    {
        ConnectorPosition position = connector.connectorPosition;

        // 벽이랑 벽을 연결할 때는 바닥을 연결 흠...
        // 그냥 가장 가까운 커텍터를 연결하면 문제가 되려나?? 이건 테스트를 해보자.
        if (currentBuildType == SelectedBuildType.wall && connector.connectorParentType == SelectedBuildType.wall)
        {
            return ConnectorPosition.bottom;
        }

        if (currentBuildType == SelectedBuildType.wall && connector.connectorParentType == SelectedBuildType.floor)
        {
            return ConnectorPosition.bottom;
        }

        if (currentBuildType == SelectedBuildType.floor && connector.connectorParentType == SelectedBuildType.wall && connector.connectorPosition == ConnectorPosition.top)
        {
            if (connector.transform.root.rotation.y == 0)
            {
                return getConnectorClosestToPlayer(true);
            }
            else
            {
                return getConnectorClosestToPlayer(false);
            }
        }

        switch (position)
        {
            case ConnectorPosition.left:
                return ConnectorPosition.right;

            case ConnectorPosition.right:
                return ConnectorPosition.left;

            case ConnectorPosition.bottom:
                return ConnectorPosition.top;

            case ConnectorPosition.top:
                return ConnectorPosition.bottom;
            default:
                return ConnectorPosition.bottom;
        }
    }

    private ConnectorPosition getConnectorClosestToPlayer(bool topBottom)
    {
        Transform cameraTransform = Camera.main.transform;

        if (topBottom)
        {
            return cameraTransform.position.z >= ghostBuildGameObject.transform.position.z ? ConnectorPosition.bottom : ConnectorPosition.top;
        }
        else
        {
            return cameraTransform.position.x >= ghostBuildGameObject.transform.position.x ? ConnectorPosition.left : ConnectorPosition.right;
        }
    }

    private void ghostifyModel(Transform modelParent, Material ghostMaterial = null)
    {
        if (ghostMaterial != null)
        {
            foreach (MeshRenderer meshRenderer in modelParent.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = ghostMaterial;
            }
        }
        else
        {
            foreach (Collider modelCollider in modelParent.GetComponentsInChildren<Collider>())
            {
                modelCollider.enabled = false;
            }
        }
    }

    private GameObject getCurrentBuild()
    {
        switch (currentBuildType)
        {
            case SelectedBuildType.floor:
                return floorObjects[currentBuildingIndex];
            case SelectedBuildType.wall:
                return wallObjects[currentBuildingIndex];
        }

        return null;
    }

    private void placeBuild()
    { 
        if(ghostBuildGameObject != null && isGhostInvalidPosition)
        {
            GameObject newBuild = Instantiate(getCurrentBuild(), ghostBuildGameObject.transform.position, ghostBuildGameObject.transform.rotation);

            Destroy(ghostBuildGameObject);
            ghostBuildGameObject = null;

            isBuilding = false;

            foreach (Connector connector in newBuild.GetComponentsInChildren<Connector>())
            { 
                connector.updateConnectors(true);
            }
        }
    }

    private void ghostDestroy()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.root.CompareTag("Buildables"))
            {
                if (!lastHitDestroyTransform)
                {
                    lastHitDestroyTransform = hit.transform.root;

                    LastHitMaterials.Clear();
                    foreach (MeshRenderer lastHitMeshRenderers in lastHitDestroyTransform.GetComponentsInChildren<MeshRenderer>())
                    {
                        LastHitMaterials.Add(lastHitMeshRenderers.material);
                    }

                    ghostifyModel(lastHitDestroyTransform.GetChild(0), ghostMaterialInvalid);
                }
                else if (hit.transform.root != lastHitDestroyTransform)
                {
                    resetLastDestroyTransform();
                }
            }
            else if (lastHitDestroyTransform)
            {
                resetLastDestroyTransform();
            }
        }
    }

    private void resetLastDestroyTransform()
    {
        int counter = 0;
        foreach (MeshRenderer lastHitMeshRenderer in lastHitDestroyTransform.GetComponentsInChildren<MeshRenderer>())
        {
            lastHitMeshRenderer.material = LastHitMaterials[counter];
            counter++;
        }

        lastHitDestroyTransform = null;
    }

    private void destroyBuild()
    {
        if (lastHitDestroyTransform)
        {

            //... 어차피 뽀갤건데, connector는 왜 비활성화 하는거지...? 뭔 코드야 이건.
            foreach (Connector connector in lastHitDestroyTransform.GetComponentsInChildren<Connector>())
            { 
                connector.gameObject.SetActive(false);
                connector.updateConnectors(true);
            }

            Destroy(lastHitDestroyTransform.gameObject);

            isDestroying = false;
            lastHitDestroyTransform = null;
        }
    }
}


[Serializable]
public enum SelectedBuildType
{ 
    floor,
    wall
}