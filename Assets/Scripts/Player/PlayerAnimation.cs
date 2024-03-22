using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAnimation : NetworkBehaviour
{
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private Animator animator;

    private PlayerBase playerBase;

    private void Awake()
    {
        playerBase = GetComponent<PlayerBase>();
        animator = playerCharacter.GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (!animator) { return; }
        animator.SetBool("IsRun", playerBase.isRun.Value);
        animator.SetBool("IsSprint", playerBase.isSprint.Value);
        animator.SetBool("IsCrouch", playerBase.isCrouch.Value);
        animator.SetBool("IsJump", playerBase.isJump.Value);
        animator.SetBool("IsFall", playerBase.isFall.Value);
        animator.SetBool("IsGround", playerBase.isGround.Value);
    }
}