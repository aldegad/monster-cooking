using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Cinemachine;
using UnityEngine.UI;
using System.Linq;

public class BuildingManager : MonoBehaviour
{
    [Header("Build Objects")]
    [SerializeField] private BuildableModule floorModule;
    [SerializeField] private BuildableModule wallModule;
    [SerializeField] private List<BuildableGroup> buildableGroups = new List<BuildableGroup>();

    [Header("Build Settings")]
    [SerializeField] private LayerMask buildableLayers;
    [SerializeField] private BuildType currentBuildType;
    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private LayerMask connectorLayer;
    [SerializeField] private BuildingUI buildingUI;

    [Header("Destroy Settings")]
    [SerializeField] private bool isDestroying = false;
    private Transform lastHitDestroyTransform;
    private List<Material[]> LastHitMaterials = new List<Material[]>();


    [Header("Ghost Settings")]
    [SerializeField] private Material ghostMaterialValid;
    [SerializeField] private Material ghostMaterialInvalid;
    [SerializeField] private float maxGroundAngle = 45f;

    [Header("Internal State")]
    [SerializeField] private bool isBuilding = false;
    [SerializeField] private float currentGroundAngle;
    [SerializeField] private int currentBuildableGroupIndex;
    [SerializeField] private int currentBuildableIndex;


    private BuildableModule ghostBuildableModule;
    private bool isGhostValidPosition = false;

    public static BuildingManager Instance { get; private set; }
    public List<BuildableGroup> BuildableGroups => buildableGroups;
    public BuildingUI BuildingUI => buildingUI;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (GameManager.Instance.GameState == GameState.Exploration || GameManager.Instance.GameState == GameState.Building)
            {
                toggleBuildingUI(true);
            }
            else
            {
                toggleBuildingUI(false);
            }
        }

        if (isBuilding && !isDestroying)
        {
            ghostBuild();

            if (Input.GetMouseButtonDown(0))
            {
                placeBuild();
            }
        }

        if (GameManager.Instance.GameState == GameState.BuildingUI)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                GameManager.Instance.GameState = GameState.Building;
                destroyBuildingToggle(true);
            }
        }

        if (isDestroying)
        {
            ghostDestroy();

            if (Input.GetMouseButtonDown(0))
            {
                destroyBuild();
            }
        }

        if (!isBuilding && !isDestroying && ghostBuildableModule != null)
        {
            Destroy(ghostBuildableModule.gameObject);
        }

        if (!isDestroying && lastHitDestroyTransform)
        {
            resetLastDestroyTransform();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isBuilding = false;
            toggleBuildingUI(false);
            destroyBuildingToggle(false);
        }
    }

    private void ghostBuild()
    {
        if (ghostBuildableModule == null)
        {
            BuildableModule currentBuild = getCurrentBuild(transform);
            createGhostPrefab(currentBuild);
        }

        moveGhostPrefabToRaycast();
        checkBuildValidity();
    }

    private void createGhostPrefab(BuildableModule currentBuild)
    {
        ghostBuildableModule = currentBuild;

        ghostifyModel(ghostBuildableModule.ModelParent, ghostMaterialValid);
        ghostifyModel(ghostBuildableModule.ModelParent);
    }

    private void moveGhostPrefabToRaycast()
    {
        // ȭ�� �߾ӿ��� ���̸� �����մϴ�.
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, buildableLayers))
        {
            // ����ĳ��Ʈ�� � ������Ʈ�� �¾Ҵٸ�, 'ghostBuildGameObject'�� ��ġ�� ���� �������� �̵���ŵ�ϴ�.
            ghostBuildableModule.transform.position = hit.point;
        }
    }

    void OnDrawGizmos()
    {
        if (ghostBuildableModule != null)
        {
            // ��ü ����� ��踦 ����մϴ�.
            Bounds bounds = CalculateTotalBounds(ghostBuildableModule.ModelParent);

            Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, ghostBuildableModule.transform.rotation, connectorLayer);
            Collider[] newColliders = RemoveMyConnector(ghostBuildableModule, colliders);

            // ���� ���� ����
            Gizmos.color = Color.red;

            foreach (Collider collider in newColliders)
            {
                SphereCollider sphereCollider = collider as SphereCollider;
                // SphereCollider�� ���� �������� ����� ���� �������� ����մϴ�.
                float scaledRadius = sphereCollider.radius * sphereCollider.transform.lossyScale.x;

                // Gizmos�� SphereCollider�� ���� ũ�⿡ ���� �׸��ϴ�.
                Gizmos.DrawWireSphere(collider.transform.position, scaledRadius);
            }

            // Gizmos�� �׸� �� ����� ���� ����
            Gizmos.color = Color.green;

            // Gizmos�� ��Ʈ������ ������Ʈ�� ���� ��ġ�� ȸ������ �����մϴ�.
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(bounds.center, ghostBuildableModule.transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;

            // ���� ��ü ��踦 ������� ���̾������� ���ڸ� �׸��ϴ�.
            Gizmos.DrawWireCube(Vector3.zero, bounds.size);
        }
    }

    private void checkBuildValidity()
    {
        Bounds bounds = CalculateTotalBounds(ghostBuildableModule.ModelParent);

        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, ghostBuildableModule.transform.rotation, connectorLayer);

        Collider[] newColliders = RemoveMyConnector(ghostBuildableModule, colliders);
        if (newColliders.Length > 0)
        {
            ghostConnectBuild(newColliders);
        }
        else
        {
            ghostSeperateBuild();

            if (isGhostValidPosition)
            {
                Collider[] overlapColliders = Physics.OverlapBox(bounds.center, bounds.extents, ghostBuildableModule.transform.rotation, buildingLayer);
                foreach (Collider overapCollider in overlapColliders)
                {
                    if (overapCollider.transform.root.gameObject != ghostBuildableModule.gameObject && overapCollider.transform.root.CompareTag("Buildables"))
                    {
                        ghostifyModel(ghostBuildableModule.ModelParent, ghostMaterialInvalid);
                        isGhostValidPosition = false;
                        return;
                    }
                }
            }
        }
    }
    private Bounds CalculateTotalBounds(GameObject modalParent)
    {
        Renderer[] renderers = modalParent.GetComponentsInChildren<Renderer>();

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private Collider[] RemoveMyConnector(BuildableModule ghostBuildGameObject, Collider[] colliders)
    {
        Connector[] myConnectors = ghostBuildGameObject.transform.GetChild(1).GetComponentsInChildren<Connector>();
        Collider[] myConnectorColliders = myConnectors.Select(connector => connector.GetComponent<Collider>()).ToArray();

        return colliders.Where(collider => !myConnectorColliders.Contains(collider)).ToArray();
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
            }
        }

        if (bestConnector == null ||
            currentBuildType == BuildType.floor && bestConnector.isConnectedToFloor ||
            currentBuildType == BuildType.wall && bestConnector.isConnectedToWall)
        {
            ghostifyModel(ghostBuildableModule.ModelParent, ghostMaterialInvalid);
            isGhostValidPosition = false;
            return;
        }

        snapGhostPrefabToConnector(bestConnector);
    }

    private void snapGhostPrefabToConnector(Connector connector)
    {
        Transform ghostConnector = findSnapConnector(connector.transform, ghostBuildableModule.transform.GetChild(1));
        ghostBuildableModule.transform.position = connector.transform.position - (ghostConnector.position - ghostBuildableModule.transform.position);

        if (currentBuildType == BuildType.wall)
        {
            Quaternion newRotation = ghostBuildableModule.transform.rotation;
            newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.rotation.eulerAngles.y, newRotation.eulerAngles.z);
            ghostBuildableModule.transform.rotation = newRotation;
        }

        ghostifyModel(ghostBuildableModule.ModelParent, ghostMaterialValid);
        isGhostValidPosition = true;
    }

    private void ghostSeperateBuild()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if(currentBuildType == BuildType.wall)
            {
                ghostifyModel(ghostBuildableModule.ModelParent, ghostMaterialInvalid);
                isGhostValidPosition = false;
                return;
            }

            currentGroundAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (currentGroundAngle < maxGroundAngle)
            {
                ghostifyModel(ghostBuildableModule.ModelParent, ghostMaterialValid);
                isGhostValidPosition = true;
            }
            else
            {
                ghostifyModel(ghostBuildableModule.ModelParent, ghostMaterialInvalid);
                isGhostValidPosition = false;
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

    // ���� �� �����ϰ� �����ؾ��ϴ� �κ��ϵ�
    private ConnectorPosition getOppositePosition(Connector connector)
    {
        ConnectorPosition position = connector.connectorPosition;

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
            return cameraTransform.position.z >= ghostBuildableModule.transform.position.z ? ConnectorPosition.bottom : ConnectorPosition.top;
        }
        else
        {
            return cameraTransform.position.x >= ghostBuildableModule.transform.position.x ? ConnectorPosition.left : ConnectorPosition.right;
        }
    }

    private void ghostifyModel(GameObject modelParent, Material ghostMaterial = null)
    {
        if (ghostMaterial != null)
        {
            foreach (MeshRenderer meshRenderer in modelParent.GetComponentsInChildren<MeshRenderer>())
            {
                Material[] ghostMaterials = new Material[meshRenderer.materials.Length];
                for (int i = 0; i < meshRenderer.materials.Length; i++)
                {
                    ghostMaterials[i] = ghostMaterial;
                }
                meshRenderer.materials = ghostMaterials;
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

    private BuildableModule getCurrentBuild(Transform transform)
    {
        BuildableObject buildableObject = buildableGroups[currentBuildableGroupIndex].buildableObjects[currentBuildableIndex];
        buildableObject.buildableObject.layer = LayerMask.NameToLayer("Building");
        switch (currentBuildType)
        {
            case BuildType.floor:
                BuildableModule floorInstance = Instantiate(floorModule, transform.position, transform.rotation);
                floorInstance.gameObject.name = buildableObject.name;
                Instantiate(buildableObject.buildableObject, floorInstance.ModelParent.transform);

                return floorInstance;

            case BuildType.wall:
                BuildableModule wallInstance = Instantiate(wallModule, transform.position, transform.rotation);
                wallInstance.gameObject.name = buildableObject.name;
                Instantiate(buildableObject.buildableObject, wallInstance.ModelParent.transform);

                return wallInstance;
        }

        return null;
    }

    private void placeBuild()
    { 
        if(ghostBuildableModule != null && isGhostValidPosition)
        {
            foreach (Connector connector in ghostBuildableModule.GetComponentsInChildren<Connector>())
            {
                connector.gameObject.SetActive(false);
            }

            BuildableModule newBuild = getCurrentBuild(ghostBuildableModule.transform);

            Destroy(ghostBuildableModule.gameObject);
            ghostBuildableModule = null;

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
                        LastHitMaterials.Add(lastHitMeshRenderers.materials);
                    }

                    ghostifyModel(lastHitDestroyTransform.GetComponent<BuildableModule>().ModelParent, ghostMaterialInvalid);
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
            lastHitMeshRenderer.materials = LastHitMaterials[counter];
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
        if (active)
        {
            GameManager.Instance.GameState = GameState.BuildingUI;
            isBuilding = false;
            isDestroying = false;
        }
        else
        {   if (isBuilding || isDestroying)
            {
                GameManager.Instance.GameState = GameState.Building;
            }
            else
            {
                GameManager.Instance.GameState = GameState.Exploration;
            }
        }

        buildingUI.gameObject.SetActive(active);

        if (active)
        {
            GameManager.Instance.PauseCamera();
        }
        else
        {
            GameManager.Instance.ResumeCamera();
        }
    }

    public void destroyBuildingToggle(bool active)
    {
        isDestroying = active;
        toggleBuildingUI(false);
    }

    public void ChangeBuildingTypeButton(BuildType selectedBuildType)
    {
        currentBuildType = selectedBuildType;
    }

    public void StartBuildingButton(int buildableGroupIndex, int buildableIndex)
    {
        GameManager.Instance.GameState = GameState.Building;
        currentBuildableGroupIndex = buildableGroupIndex;
        currentBuildableIndex = buildableIndex;

        isBuilding = true;
        toggleBuildingUI(false);
    }
}


[Serializable]
public enum BuildType
{ 
    floor,
    wall,
    roof,
    stair,
    furniture
}

[Serializable]
public class BuildableGroup
{
    public string buildableGroupName;
    public List<BuildableObject> buildableObjects;
}

