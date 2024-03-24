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
    [SerializeField] private List<BuildableGroup> _buildableGroups = new List<BuildableGroup>();

    [Header("Build Settings")]
    [SerializeField] private LayerMask buildableLayers;
    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private LayerMask connectorLayer;
    [SerializeField] private BuildingUI _buildingUI;
    [SerializeField] private AudioClip[] buildableSpawnSounds;

    [Header("Destroy Settings")]
    private Transform lastHitDestroyTransform;
    private List<Material[]> LastHitMaterials = new List<Material[]>();


    [Header("Ghost Settings")]
    [SerializeField] private Material _ghostMaterialValid;
    [SerializeField] private Material _ghostMaterialInvalid;
    [SerializeField] private float maxGroundAngle = 45f;

    [Header("Internal State")]
    [SerializeField] private bool isBuilding = false;
    [SerializeField] private bool isDestroying = false;
    [SerializeField] private BuildableModule ghostModule;
    [SerializeField] private float currentGroundAngle;
    [SerializeField] private int currentBuildableGroupIndex;
    [SerializeField] private int currentBuildableIndex;

    public static BuildingManager Instance { get; private set; }
    public List<BuildableGroup> buildableGroups => _buildableGroups;
    public BuildingUI buildingUI => _buildingUI;
    public Material ghostMaterialValid => _ghostMaterialValid;
    public Material ghostMaterialInvalid => _ghostMaterialInvalid;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance.gameScene != GameScene.GameScene) { return; }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (GameManager.Instance.gameState == GameState.Exploration || GameManager.Instance.gameState == GameState.Building)
            {
                toggleBuildingUI(true);
            }
            else
            {
                toggleBuildingUI(false);
            }
        }

        if (isBuilding && !isDestroying && ghostModule)
        {
            ghostBuild();

            if (Input.GetMouseButtonDown(0))
            {
                spawnBuild();
            }
        }

        if (GameManager.Instance.gameState == GameState.BuildingUI)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                GameManager.Instance.gameState = GameState.Building;
                destroyBuildingToggle(true);
            }
        }

        if (isDestroying)
        {
            ghostDestroy();

            if (Input.GetMouseButtonDown(0))
            {
                DespawnBuild();
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
    }

    private void OnDrawGizmos()
    {
        if (ghostModule != null)
        {
            // 전체 모듈의 경계를 계산합니다.
            Bounds bounds = CalculateTotalBounds(ghostModule.modelParent);

            Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, ghostModule.transform.rotation, connectorLayer);
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
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(bounds.center, ghostModule.transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;

            // 계산된 전체 경계를 기반으로 와이어프레임 상자를 그립니다.
            Gizmos.DrawWireCube(Vector3.zero, bounds.size);
        }
    }

    private void ghostBuild()
    {
        moveGhostPrefabToRaycast();
        checkBuildValidity();
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
        Bounds bounds = CalculateTotalBounds(ghostModule.modelParent);
        
        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, ghostModule.transform.rotation, connectorLayer);
        
        Collider[] newColliders = RemoveGhostConnector(colliders);
        if (newColliders.Length > 0)
        {
            ghostConnectBuild(newColliders);
        }
        else
        {
            ghostSeperateBuild();

            if (ghostModule.isGhostValidPosition.Value)
            {
                Collider[] overlapColliders = Physics.OverlapBox(bounds.center, bounds.extents, ghostModule.transform.rotation, buildingLayer);
                foreach (Collider overapCollider in overlapColliders)
                {
                    if (overapCollider.transform.root.gameObject != ghostModule.gameObject && overapCollider.transform.root.CompareTag("Buildables"))
                    {
                        ghostModule.isGhostValidPosition.Value = false;
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
            ghostModule.buildType == BuildType.floor && bestConnector.isConnectedToFloor ||
            ghostModule.buildType == BuildType.wall && bestConnector.isConnectedToWall)
        {
            ghostModule.isGhostValidPosition.Value = false;
            return;
        }

        snapGhostPrefabToConnector(bestConnector);
    }

    private void snapGhostPrefabToConnector(Connector connector)
    {
        Transform ghostConnector = findSnapConnector(connector.transform, ghostModule.connectorParent);
        ghostModule.transform.position = connector.transform.position - (ghostConnector.position - ghostModule.transform.position);

        if (ghostModule.buildType == BuildType.wall)
        {
            Quaternion newRotation = ghostModule.transform.rotation;
            newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, connector.transform.rotation.eulerAngles.y, newRotation.eulerAngles.z);
            ghostModule.transform.rotation = newRotation;
        }

        ghostModule.isGhostValidPosition.Value = true;
    }

    private void ghostSeperateBuild()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if(ghostModule.buildType == BuildType.wall)
            {
                ghostModule.isGhostValidPosition.Value = false;
                return;
            }

            currentGroundAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (currentGroundAngle < maxGroundAngle)
            {
                ghostModule.isGhostValidPosition.Value = true;
            }
            else
            {
                ghostModule.isGhostValidPosition.Value = false;
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

        if (ghostModule.buildType == BuildType.wall && connector.connectorParentType == BuildType.floor)
        {
            return ConnectorPosition.bottom;
        }

        if (ghostModule.buildType == BuildType.floor && connector.connectorParentType == BuildType.wall && connector.connectorPosition == ConnectorPosition.top)
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

    private void spawnGhostBuild(Transform transform, int currentBuildableGroupIndex, int currentBuildableIndex)
    {
        spawnGhostBuildServerRpc(transform.position, transform.rotation, currentBuildableGroupIndex, currentBuildableIndex);
    }
    [ServerRpc(RequireOwnership = false)]
    private void spawnGhostBuildServerRpc(Vector3 position, Quaternion rotation, int currentBuildableGroupIndex, int currentBuildableIndex, ServerRpcParams rpcParams = default)
    {
        BuildableModule module = buildableGroups[currentBuildableGroupIndex].buildableModules[currentBuildableIndex];
        BuildableModule moduleInstance = Instantiate(module, position, rotation);
        NetworkObject networkObject = moduleInstance.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(rpcParams.Receive.SenderClientId);

        spawnGhostBuildClientRpc(networkObject, ServerManager.Instance.TargetClient(rpcParams.Receive.SenderClientId));
    }
    [ClientRpc]
    private void spawnGhostBuildClientRpc(NetworkObjectReference reference, ClientRpcParams _ = default)
    {
        reference.TryGet(out NetworkObject networkObject);
        ghostModule = networkObject.GetComponent<BuildableModule>();
        ghostModule.isGhost.Value = true;
    }

    private void spawnBuild()
    { 
        if(ghostModule != null && ghostModule.isGhostValidPosition.Value)
        {
            spawnBuildServerRpc(ghostModule.transform.position, ghostModule.transform.rotation, currentBuildableGroupIndex, currentBuildableIndex);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void spawnBuildServerRpc(Vector3 position, Quaternion rotation, int currentBuildableGroupIndex, int currentBuildableIndex)
    {
        BuildableModule module = buildableGroups[currentBuildableGroupIndex].buildableModules[currentBuildableIndex];
        BuildableModule moduleInstance = Instantiate(module, position, rotation);
        NetworkObject networkObject = moduleInstance.GetComponent<NetworkObject>();
        networkObject.Spawn();

        spawnBuildClientRpc(networkObject);
    }

    [ClientRpc]
    private void spawnBuildClientRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject networkObject);
        BuildableModule module = networkObject.GetComponent<BuildableModule>();
        foreach (Connector connector in module.GetComponentsInChildren<Connector>())
        {
            connector.updateConnectors(true);
        }
        //module.
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

                    ghostifyModel(lastHitDestroyTransform.GetComponent<BuildableModule>().modelParent, ghostMaterialInvalid);
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

    private void DespawnBuild()
    {
        if (lastHitDestroyTransform)
        {
            DespawnBuildServerRpc(lastHitDestroyTransform.GetComponent<NetworkObject>());
            foreach (Connector connector in lastHitDestroyTransform.GetComponentsInChildren<Connector>())
            {
                connector.isGhostParent = true;
                connector.updateConnectors(true);
            }

            Destroy(lastHitDestroyTransform.gameObject);

            destroyBuildingToggle(true);
            lastHitDestroyTransform = null;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void DespawnBuildServerRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject networkObject);
        //networkObject.
        networkObject.Despawn();
        
    }
    /*[ClientRpc]
    private void */

    public void toggleBuildingUI(bool active)
    {
        if (active)
        {
            GameManager.Instance.gameState = GameState.BuildingUI;
            isBuilding = false;
            isDestroying = false;
        }
        else
        {   if (isBuilding || isDestroying)
            {
                GameManager.Instance.gameState = GameState.Building;
            }
            else
            {
                GameManager.Instance.gameState = GameState.Exploration;
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

    public void StartBuildingButton(int buildableGroupIndex, int buildableIndex)
    {
        GameManager.Instance.gameState = GameState.Building;
        currentBuildableGroupIndex = buildableGroupIndex;
        currentBuildableIndex = buildableIndex;

        isBuilding = true;
        toggleBuildingUI(false);
        spawnGhostBuild(transform, currentBuildableGroupIndex, currentBuildableIndex);
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

