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
        animator.SetBool("IsRun", playerBase.isRun);
        animator.SetBool("IsSprint", playerBase.isSprint);
        animator.SetBool("IsCrouch", playerBase.isCrouch);
        animator.SetBool("IsJump", playerBase.isJump);
        animator.SetBool("IsFall", playerBase.isFall);
        animator.SetBool("IsGround", playerBase.isGround);
    }
}