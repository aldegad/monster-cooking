using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAnimation : NetworkBehaviour
{
    private PlayerBase playerBase;
    private PlayerCharacter playerCharacter;

    private bool lastCrouchState = false;

    private void Awake()
    {
        playerBase = GetComponent<PlayerBase>();
        playerCharacter = GetComponent<PlayerCharacter>();
    }

    private void Update()
    {
        UpdateAnimation();
        UpdateCrouchMeshBounds();
    }

    private void UpdateAnimation()
    {
        if (!playerCharacter.animator) { return; }
        playerCharacter.animator.SetBool("IsRun", playerBase.isRun);
        playerCharacter.animator.SetBool("IsSprint", playerBase.isSprint);
        playerCharacter.animator.SetBool("IsCrouch", playerBase.isCrouch);
        playerCharacter.animator.SetBool("IsJump", playerBase.isJump);
        playerCharacter.animator.SetBool("IsFall", playerBase.isFall);
        playerCharacter.animator.SetBool("IsGround", playerBase.isGround);
    }

    private void UpdateCrouchMeshBounds()
    {
        if (lastCrouchState == playerBase.isCrouch) { return; }
        lastCrouchState = playerBase.isCrouch;

        foreach (PlayerBoots boots in playerCharacter.boots)
        {
            boots.CrouchBounds(playerBase.isCrouch);
        }
    }
}
