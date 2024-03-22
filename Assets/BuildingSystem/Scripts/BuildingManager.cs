using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Cinemachine;
using UnityEngine.UI;
using System.Linq;
using Unity.Netcode;

public class BuildingManager : NetworkBehaviour
{
    [Header("Build Objects")]
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
    [SerializeField] private BuildableModule ghostModule;
    [SerializeField] private bool isGhostValidPosition;
    [SerializeField] private float currentGroundAngle;
    [SerializeField] private int currentBuildableGroupIndex;
    [SerializeField] private int currentBuildableIndex;

    [Header("Buildings")]
    [SerializeField] private List<BuildableModule> buildings = new List<BuildableModule>();

    private ulong localClientId;
    private Dictionary<ulong, BuildingGhostData> ghostDatas = new Dictionary<ulong, BuildingGhostData>();
    private bool isInitialized = false;

    public static BuildingManager Instance { get; private set; }
    public List<BuildableGroup> BuildableGroups => buildableGroups;
    public BuildingUI BuildingUI => buildingUI;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        localClientId = NetworkManager.Singleton.LocalClientId;
        InitializeServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void InitializeServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        ClientRpcParams newClientRpcParams = ServerManager.Instance.TargetClient(senderClientId);

        // new player
        foreach (var kvp in ghostDatas)
        {
            ulong clientId = kvp.Key;
            BuildingGhostData ghostData = kvp.Value;
            SyncDataClientRpc(clientId, ghostData.isBuilding, ghostData.isDestroying, ghostData.isGhostValidPosition, ghostData.currentBuildableGroupIndex, ghostData.currentBuildableIndex, ghostData.ghostPosition, ghostData.ghostRotation, newClientRpcParams);
        }

        // all players
        AddDataClientRpc(rpcParams.Receive.SenderClientId);
    }
    [ClientRpc]
    private void AddDataClientRpc(ulong senderClientId)
    {
        ghostDatas[senderClientId] = new BuildingGhostData();
        if (localClientId == senderClientId) 
        {
            isInitialized = true;
        }
    }

    [ClientRpc]
    private void SyncDataClientRpc(ulong clientId, bool isBuilding, bool isDestroying, bool isGhostValidPosition, int currentBuildableGroupIndex, int currentBuildableIndex, Vector3 ghostPosition, Quaternion ghostRotation, ClientRpcParams _ = default)
    {
        ghostDatas[clientId] = new BuildingGhostData(isBuilding, isDestroying, isGhostValidPosition, currentBuildableGroupIndex, currentBuildableIndex, ghostPosition, ghostRotation);
    }

    private void Update()
    {
        if (!isInitialized) { return; }

        // 어차피 얘는 오너거 하나밖에 없음. 플레이어 오브젝트와 달리 각자 하나씩 가지고 있음.
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

        if (!isBuilding && !isDestroying && ghostModule != null)
        {
            Destroy(ghostModule.gameObject);
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

        UpdateInternalState();
    }

    private void UpdateInternalState()
    {
        if (ghostModule)
        {
            UpdateInternalStateServerRpc(isBuilding, isDestroying, isGhostValidPosition, currentBuildableGroupIndex, currentBuildableIndex, ghostModule.transform.position, ghostModule.transform.rotation);
        }
        else
        {
            UpdateInternalStateServerRpc(isBuilding, isDestroying, isGhostValidPosition, currentBuildableGroupIndex, currentBuildableIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateInternalStateServerRpc(bool isBuilding, bool isDestroying, bool isGhostValidPosition, int currentBuildableGroupIndex, int currentBuildableIndex, Vector3 ghostPosition = new Vector3(), Quaternion ghostRotation = new Quaternion(), ServerRpcParams rpcParams = default)
    {
        UpdateInternalStateClientRpc(rpcParams.Receive.SenderClientId, isBuilding, isDestroying, isGhostValidPosition, currentBuildableGroupIndex, currentBuildableIndex, ghostPosition, ghostRotation);
    }

    [ClientRpc]
    private void UpdateInternalStateClientRpc(ulong senderClientId, bool isBuilding, bool isDestroying, bool isGhostValidPosition, int currentBuildableGroupIndex, int currentBuildableIndex, Vector3 ghostPosition, Quaternion ghostRotation)
    {
        if (!ghostDatas.ContainsKey(senderClientId)) { return; }

        bool prevGhostValidPosition = ghostDatas[senderClientId].isGhostValidPosition;
        ghostDatas[senderClientId].isBuilding = isBuilding;
        ghostDatas[senderClientId].isDestroying = isDestroying;
        ghostDatas[senderClientId].isGhostValidPosition = isGhostValidPosition;
        ghostDatas[senderClientId].currentBuildableGroupIndex = currentBuildableGroupIndex;
        ghostDatas[senderClientId].currentBuildableIndex = currentBuildableIndex;
        ghostDatas[senderClientId].ghostPosition = ghostPosition;
        ghostDatas[senderClientId].ghostRotation = ghostRotation;

        if (localClientId == senderClientId) 
        {
            ghostDatas[senderClientId].ghostModule = ghostModule;
            return; 
        }

        if (isBuilding && !isDestroying)
        {
            if (ghostDatas[senderClientId].ghostModule == null)
            {
                BuildableModule currentBuild = getCurrentBuild(transform, currentBuildableGroupIndex, currentBuildableIndex);
                ghostDatas[senderClientId].ghostModule = currentBuild;
                ghostDatas[senderClientId].ghostModule.isGhost = true;

                ghostifyModel(currentBuild.ModelParent, ghostMaterialValid);
                ghostifyModel(currentBuild.ModelParent);
            }

            ghostDatas[senderClientId].ghostModule.transform.position = ghostPosition;
            ghostDatas[senderClientId].ghostModule.transform.rotation = ghostRotation;

            if (prevGhostValidPosition != isGhostValidPosition)
            {
                ghostifyModel(ghostDatas[senderClientId].ghostModule.ModelParent, isGhostValidPosition ? ghostMaterialValid : ghostMaterialInvalid);
            }
        }

        if (!isBuilding && !isDestroying && ghostDatas[senderClientId].ghostModule != null)
        {
            Destroy(ghostDatas[senderClientId].ghostModule.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var kvp in ghostDatas)
        {
            ulong clientId = kvp.Key;
            BuildableModule module = kvp.Value.ghostModule;

            if (module != null && module.ModelParent != null)
            {
                // 전체 모듈의 경계를 계산합니다.
                Bounds bounds = CalculateTotalBounds(module.ModelParent);

                Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, module.transform.rotation, connectorLayer);
                Collider[] newColliders = RemoveGhostConnector(colliders);

                // 원의 색상 설정
                Gizmos.color = Color.red;

                foreach (Collider collider in newColliders)
                {
                    if (collider is SphereCollider sphereCollider)
                    {
                        // SphereCollider의 월드 스케일을 고려한 실제 반지름을 계산합니다.
                        float scaledRadius = sphereCollider.radius * Mathf.Max(sphereCollider.transform.lossyScale.x, sphereCollider.transform.lossyScale.y, sphereCollider.transform.lossyScale.z);

                        // Gizmos를 SphereCollider의 실제 크기에 맞춰 그립니다.
                        Gizmos.DrawWireSphere(collider.transform.position, scaledRadius);
                    }
                }

                // Gizmos를 그릴 때 사용할 색상 설정
                Gizmos.color = Color.red;

                // Gizmos의 매트릭스를 오브젝트의 월드 위치와 회전으로 설정합니다.
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(bounds.center, module.transform.rotation, Vector3.one);
                Gizmos.matrix = rotationMatrix;

                // 계산된 전체 경계를 기반으로 와이어프레임 상자를 그립니다.
                Gizmos.DrawWireCube(Vector3.zero, bounds.size);
            }
        }
    }

    private void ghostBuild()
    {
        if (ghostModule == null)
        {
            BuildableModule currentBuild = getCurrentBuild(transform, currentBuildableGroupIndex, currentBuildableIndex);
            createGhostPrefab(currentBuild);
        }

        moveGhostPrefabToRaycast();
        checkBuildValidity();
    }

    private void createGhostPrefab(BuildableModule currentBuild)
    {
        ghostModule = currentBuild;
        ghostModule.isGhost = true;
        ghostifyModel(currentBuild.ModelParent, ghostMaterialValid);
        ghostifyModel(currentBuild.ModelParent);
    }

    private void moveGhostPrefabToRaycast()
    {
        // 화면 중앙에서 레이를 생성합니다.
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, buildableLayers))
        {
            // 레이캐스트가 어떤 오브젝트에 맞았다면, 'ghostBuildGameObject'의 위치를 맞은 지점으로 이동시킵니다.
            ghostModule.transform.position = hit.point;
        }
    }

    private void checkBuildValidity()
    {
        Bounds bounds = CalculateTotalBounds(ghostModule.ModelParent);
        
        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, ghostModule.transform.rotation, connectorLayer);
        
        Collider[] newColliders = RemoveGhostConnector(colliders);
        if (newColliders.Length > 0)
        {
            ghostConnectBuild(newColliders);
        }
        else
        {
            ghostSeperateBuild();

            if (isGhostValidPosition)
            {
                Collider[] overlapColliders = Physics.OverlapBox(bounds.center, bounds.extents, ghostModule.transform.rotation, buildingLayer);
                foreach (Collider overapCollider in overlapColliders)
                {
                    if (overapCollider.transform.root.gameObject != ghostModule.gameObject && overapCollider.transform.root.CompareTag("Buildables"))
                    {
                        ghostifyModel(ghostModule.ModelParent, ghostMaterialInvalid);
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

    private Collider[] RemoveGhostConnector(Collider[] colliders)
    {
        return colliders.Where(collider => !collider.GetComponent<Connector>().isGhostParent).ToArray();
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
            ghostifyModel(ghostModule.ModelParent, ghostMaterialInvalid);
            isGhostValidPosition = false;
            return;
        }

        snapGhostPrefabToConnector(bestConnector);
    }

    private void snapGhostPrefabToConnector(Connector connector)
    {
        Transform ghostConnector = findSnapConnector(connector.transform, ghostModule.ConnectorParent);
        ghostModule.transform.position = connector.transform.position - (ghostConnector.position - ghostModule.transform.position);

        if (currentBuildType == BuildType.wall)
        {
            Quaternion newRotation = ghostModule.transform.rotation;
            newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.rotation.eulerAngles.y, newRotation.eulerAngles.z);
            ghostModule.transform.rotation = newRotation;
        }

        ghostifyModel(ghostModule.ModelParent, ghostMaterialValid);
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
                ghostifyModel(ghostModule.ModelParent, ghostMaterialInvalid);
                isGhostValidPosition = false;
                return;
            }

            currentGroundAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (currentGroundAngle < maxGroundAngle)
            {
                ghostifyModel(ghostModule.ModelParent, ghostMaterialValid);
                isGhostValidPosition = true;
            }
            else
            {
                ghostifyModel(ghostModule.ModelParent, ghostMaterialInvalid);
                isGhostValidPosition = false;
            }
        }
    }

    private Transform findSnapConnector(Transform snapConnector, GameObject ghostConnectorParent)
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
            return cameraTransform.position.z >= ghostModule.transform.position.z ? ConnectorPosition.bottom : ConnectorPosition.top;
        }
        else
        {
            return cameraTransform.position.x >= ghostModule.transform.position.x ? ConnectorPosition.left : ConnectorPosition.right;
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

    private BuildableModule getCurrentBuild(Transform transform, int currentBuildableGroupIndex, int currentBuildableIndex)
    {
        BuildableModule module = buildableGroups[currentBuildableGroupIndex].buildableModules[currentBuildableIndex];
        return Instantiate(module, transform.position, transform.rotation);
    }

    private void placeBuild()
    { 
        if(ghostModule != null && isGhostValidPosition)
        {
            foreach (Connector connector in ghostModule.GetComponentsInChildren<Connector>())
            {
                connector.gameObject.SetActive(false);
            }

            BuildableModule newBuild = getCurrentBuild(ghostModule.transform, currentBuildableGroupIndex, currentBuildableIndex);
            placeBuildServerRpc(currentBuildableGroupIndex, currentBuildableIndex, ghostModule.transform.position, ghostModule.transform.rotation);

            Destroy(ghostModule.gameObject);
            ghostModule = null;

            foreach (Connector connector in newBuild.GetComponentsInChildren<Connector>())
            {
                connector.updateConnectors(true);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void placeBuildServerRpc(int currentBuildableGroupIndex, int currentBuildableIndex, Vector3 ghostPosition, Quaternion ghostRotation, ServerRpcParams rpcParams = default)
    {
        placeBuildClientRpc(rpcParams.Receive.SenderClientId, currentBuildableGroupIndex, currentBuildableIndex, ghostPosition, ghostRotation);
    }

    [ClientRpc]
    private void placeBuildClientRpc(ulong senderClientId, int currentBuildableGroupIndex, int currentBuildableIndex, Vector3 ghostPosition, Quaternion ghostRotation)
    {
        if (localClientId == senderClientId) { return; }

        // because data delay
        ghostDatas[senderClientId].ghostModule.transform.position = ghostPosition;
        ghostDatas[senderClientId].ghostModule.transform.rotation = ghostRotation;

        foreach (Connector connector in ghostDatas[senderClientId].ghostModule.GetComponentsInChildren<Connector>())
        {
            connector.gameObject.SetActive(false);
        }

        BuildableModule newBuild = getCurrentBuild(ghostDatas[senderClientId].ghostModule.transform, currentBuildableGroupIndex, currentBuildableIndex);

        Destroy(ghostDatas[senderClientId].ghostModule.gameObject);
        ghostDatas[senderClientId].ghostModule = null;

        foreach (Connector connector in newBuild.GetComponentsInChildren<Connector>())
        {
            connector.updateConnectors(true);
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
    public string groupName;
    public List<BuildableModule> buildableModules;
}

