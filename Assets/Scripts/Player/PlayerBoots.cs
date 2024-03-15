using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoots : MonoBehaviour
{
    [SerializeField] private Vector3 crouchBoundsCenter;

    private SkinnedMeshRenderer skinMeshRenderer;
    private Bounds originBounds;

    void Start()
    {
        skinMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        originBounds = skinMeshRenderer.localBounds;
    }

    public void CrouchBounds(bool isCrouch)
    {
        if (isCrouch)
        {
            skinMeshRenderer.localBounds = new Bounds(crouchBoundsCenter, originBounds.size);
        }
        else
        {
            skinMeshRenderer.localBounds = originBounds;
        }
    }
}
