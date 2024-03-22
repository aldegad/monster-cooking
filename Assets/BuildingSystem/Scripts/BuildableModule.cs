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
    [SerializeField] private GameObject _modelParent;
    [SerializeField] private GameObject _connectorParent;
    [SerializeField] private bool _isGhost = false;
    [SerializeField] private bool _isGhostValidPosition = true;

    public GameObject modelParent => _modelParent;
    public GameObject connectorParent => _connectorParent;

    public NetworkVariable<bool> isGhost = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isGhostValidPosition = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        isGhost.Value = _isGhost;
        isGhostValidPosition.Value = _isGhostValidPosition;

        isGhost.OnValueChanged += IsGhost_OnValueChanged;
        isGhostValidPosition.OnValueChanged += IsGhostValidPosition_OnValueChanged;
    }
    private void GhostifyModel()
    {
        foreach (Collider modelCollider in modelParent.GetComponentsInChildren<Collider>())
        {
            modelCollider.enabled = false;
        }
    }
    private void UpdateGhostMaterial(bool isGhostValidPosition)
    {
        Material material = isGhostValidPosition ? BuildingManager.Instance.ghostMaterialValid : BuildingManager.Instance.ghostMaterialInvalid;

        foreach (MeshRenderer meshRenderer in modelParent.GetComponentsInChildren<MeshRenderer>())
        {
            Material[] ghostMaterials = new Material[meshRenderer.materials.Length];
            for (int i = 0; i < meshRenderer.materials.Length; i++)
            {
                ghostMaterials[i] = material;
            }
            meshRenderer.materials = ghostMaterials;
        }
    }

    private void IsGhost_OnValueChanged(bool previous, bool current)
    {
        if (isGhost.Value)
        {
            GhostifyModel();
            UpdateGhostMaterial(true);
        }

        Connector[] connectors = connectorParent.GetComponentsInChildren<Connector>();
        foreach (Connector connector in connectors)
        {
            connector.isGhostParent = isGhost.Value;
        }
    }

    private void IsGhostValidPosition_OnValueChanged(bool previous, bool current)
    {
        UpdateGhostMaterial(isGhostValidPosition.Value);
    }
}
