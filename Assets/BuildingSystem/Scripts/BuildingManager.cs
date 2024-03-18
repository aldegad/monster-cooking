using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Cinemachine;

public class BuildingManager : MonoBehaviour
{
    [Header("Build Objects")]
    [SerializeField] private GameObject floorModule;
    [SerializeField] private GameObject wallModule;
    [SerializeField] private List<GameObject> floorObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> wallObjects = new List<GameObject>();

    [Header("Build Settings")]
    [SerializeField] private LayerMask buildableLayers;
    [SerializeField] private BuildType currentBuildType;
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

    [Header("Debug UI")]
    [SerializeField] private BuildingUI buildingUI;
    [SerializeField] private GameObject buildableObjectGrid;
    [SerializeField] private BuildableObjectButton selectBuildableObjectButton;

    private GameObject ghostBuildGameObject;
    private bool isGhostInvalidPosition = false;
    private Transform ModelParent = null;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        buildingUI.gameObject.SetActive(false);

        for (int i = 0; i < floorObjects.Count; i++)
        {
            GameObject floorObjectInstance = Instantiate(selectBuildableObjectButton.gameObject, buildableObjectGrid.transform);
            BuildableObjectButton buildableObjectButton = floorObjectInstance.GetComponent<BuildableObjectButton>();

            buildableObjectButton.buildingManager = this;
            buildableObjectButton.selectedBuildType = BuildType.floor;
            buildableObjectButton.buildingIndex = i;
        }

        for (int i = 0; i < wallObjects.Count; i++)
        {
            GameObject wallObjectInstance = Instantiate(selectBuildableObjectButton.gameObject, buildableObjectGrid.transform);
            BuildableObjectButton buildableObjectButton = wallObjectInstance.GetComponent<BuildableObjectButton>();

            buildableObjectButton.buildingManager = this;
            buildableObjectButton.selectedBuildType = BuildType.wall;
            buildableObjectButton.buildingIndex = i;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            toggleBuildingUI(!buildingUI.gameObject.activeSelf);
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
        if (ghostBuildGameObject == null)
        {
            GameObject currentBuild = getCurrentBuild(transform);
            createGhostPrefab(currentBuild);
        }

        moveGhostPrefabToRaycast();
        checkBuildValidity();
    }

    private void createGhostPrefab(GameObject currentBuild)
    {
        ghostBuildGameObject = currentBuild;

        ModelParent = ghostBuildGameObject.transform.GetChild(0);

        ghostifyModel(ModelParent, ghostMaterialValid);
        ghostifyModel(ghostBuildGameObject.transform);
    }

    private void moveGhostPrefabToRaycast()
    {
        // 화면 중앙에서 레이를 생성합니다.
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, buildableLayers))
        {
            // 레이캐스트가 어떤 오브젝트에 맞았다면, 'ghostBuildGameObject'의 위치를 맞은 지점으로 이동시킵니다.
            ghostBuildGameObject.transform.position = hit.point;
        }
    }
    void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(ray.origin, hit.point);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * 100);
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
            currentBuildType == BuildType.floor && bestConnector.isConnectedToFloor ||
            currentBuildType == BuildType.wall && bestConnector.isConnectedToWall)
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

        if (currentBuildType == BuildType.wall)
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
            if(currentBuildType == BuildType.wall)
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

    // 요기는 좀 섬세하게 조정해야하는 부분일듯
    private ConnectorPosition getOppositePosition(Connector connector)
    {
        ConnectorPosition position = connector.connectorPosition;

        /*if (currentBuildType == BuildType.wall && connector.connectorParentType == BuildType.wall)
        {
            return ConnectorPosition.bottom;
        }*/

        if (currentBuildType == BuildType.wall && connector.connectorParentType == BuildType.floor)
        {
            return ConnectorPosition.bottom;
        }

        if (currentBuildType == BuildType.floor && connector.connectorParentType == BuildType.wall && connector.connectorPosition == ConnectorPosition.top)
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

    private GameObject getCurrentBuild(Transform transform = null)
    {
        switch (currentBuildType)
        {
            case BuildType.floor:
                GameObject floorInstance = Instantiate(floorModule, transform.position, transform.rotation);
                Transform floorModelParent = floorInstance.transform.GetChild(0);
                floorInstance.name = floorObjects[currentBuildingIndex].name;
                Instantiate(floorObjects[currentBuildingIndex], floorModelParent.transform);

                return floorInstance;

            case BuildType.wall:
                GameObject wallInstance = Instantiate(wallModule, transform.position, transform.rotation);
                Transform wallModelParent = wallInstance.transform.GetChild(0);
                wallInstance.name = wallObjects[currentBuildingIndex].name;
                Instantiate(wallObjects[currentBuildingIndex], wallModelParent.transform);

                return wallInstance;
        }

        return null;
    }

    private void placeBuild()
    { 
        if(ghostBuildGameObject != null && isGhostInvalidPosition)
        {
            GameObject newBuild = getCurrentBuild(ghostBuildGameObject.transform);

            Destroy(ghostBuildGameObject);
            ghostBuildGameObject = null;

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
            foreach (Connector connector in lastHitDestroyTransform.GetComponentsInChildren<Connector>())
            { 
                connector.gameObject.SetActive(false);
                connector.updateConnectors(true);
            }

            Destroy(lastHitDestroyTransform.gameObject);

            destroyBuildingToggle(true);
            lastHitDestroyTransform = null;
        }
    }

    public void toggleBuildingUI(bool active)
    {
        isBuilding = false;

        buildingUI.gameObject.SetActive(active);

        CinemachineFreeLook freelook = FindFirstObjectByType<CinemachineFreeLook>();
        if (active)
        {
            freelook.m_XAxis.m_MaxSpeed = 0;
            freelook.m_YAxis.m_MaxSpeed = 0;
        }
        else
        {
            freelook.m_XAxis.m_MaxSpeed = 500;
            freelook.m_YAxis.m_MaxSpeed = 4;
        }

        Cursor.visible = active;
        Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void destroyBuildingToggle(bool fromScript = false)
    {
        Color green;
        Color red;
        ColorUtility.TryParseHtmlString("#FF4747", out red);
        ColorUtility.TryParseHtmlString("#48FF62", out green);

        if (fromScript)
        {
            isDestroying = false;
            buildingUI.DestroyText.text = "Destroy Off";
            buildingUI.DestroyText.color = green;
        }
        else
        {
            isDestroying = !isDestroying;
            buildingUI.DestroyText.text = isDestroying ? "Destroy On" : "Destroy Off";
            buildingUI.DestroyText.color = isDestroying ? red : green;
            toggleBuildingUI(false);
        }
    }

    public void changeBuildingTypeButton(BuildType selectedBuildType)
    {
        currentBuildType = selectedBuildType;
    }

    public void startBuildingButton(int buildIndex)
    {
        currentBuildingIndex = buildIndex;
        toggleBuildingUI(false);

        isBuilding = true;
    }
}


[Serializable]
public enum BuildType
{ 
    floor,
    wall
}