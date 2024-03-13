using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerAnimation : NetworkBehaviour
{
    [SerializeField] public bool isRun = false;
    [SerializeField] public bool isSprint = false;
    [SerializeField] public bool isCrouch = false;

    private PlayerCharacter playerCharacter;
    private void Awake()
    {
        playerCharacter = GetComponent<PlayerCharacter>();
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        UpdateAnimationState();
        UpdateAnimation();
    }

    private void UpdateAnimationState()
    {
        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isRun = false;
                isSprint = true;
            }
            else
            {
                isRun = true;
                isSprint = false;
            }
        }
        else
        {
            isRun = false;
            isSprint = false;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouch = !isCrouch;
        }

        // 내꺼는 빨리 업뎃되어야 하니깐.
        playerCharacter.animator.SetBool("IsRun", isRun);
        playerCharacter.animator.SetBool("IsSprint", isSprint);
        playerCharacter.animator.SetBool("IsSprint", isCrouch);
    }

    private void UpdateAnimation()
    {
        UpdateAnimationServerRpc(isRun, isSprint, isCrouch);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateAnimationServerRpc(bool isRun, bool isSprint, bool isCrouch)
    {
        UpdateAnimationClientRpc(isRun, isSprint, isCrouch);
    }

    [ClientRpc]
    private void UpdateAnimationClientRpc(bool isRun, bool isSprint, bool isCrouch)
    { 
        this.isRun = isRun;
        this.isSprint = isSprint;
        this.isCrouch = isCrouch;

        // 다른 사람들을 위한 업데이트
        playerCharacter.animator.SetBool("IsRun", isRun);
        playerCharacter.animator.SetBool("IsSprint", isSprint);
        playerCharacter.animator.SetBool("IsCrouch", isCrouch);
    }
}
