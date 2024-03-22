using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Spawn Point")]
    [SerializeField] private Vector3 spawnPoint;

    [Header("Movement")]
    [SerializeField] public float gravity = 3f;
    [SerializeField] public float runSpeed = 5f;
    [SerializeField] public float sprintSpeed = 8f;
    [SerializeField] public float crouchSpeed = 3f;
    [SerializeField] public float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 100f;

    private PlayerBase playerBase;
    private Rigidbody rigid;

    private NetworkVariable<bool> isInitialized = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Vector3 moveDirection;
    private float speed = 0f;

    private bool jumpable = true;

    private void Awake()
    {
        playerBase = GetComponent<PlayerBase>();
        rigid = GetComponent<Rigidbody>();
        rigid.useGravity = false;
        speed = runSpeed;
    }

    private void Update()
    {
        if (GameManager.Instance.gameScene != GameScene.GameScene) { return; }
        if (!IsOwner) { return; }
        
        initializePosition();

        if (!isInitialized.Value) { return; }

        UpdateSpeed();
        UpdateMoveDirection();
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance.gameScene != GameScene.GameScene) { return; }
        if (!IsOwner || !isInitialized.Value) { return; }

        UpdatePosition();
    }

    private void initializePosition()
    {
        if (isInitialized.Value) { return; }

        transform.position = spawnPoint;
        isInitialized.Value = true;
        initializePositionServerRpc();
    }
    [ServerRpc]
    private void initializePositionServerRpc()
    {
        initializePositionClientRpc();
    }
    [ClientRpc]
    private void initializePositionClientRpc()
    {
        rigid.useGravity = true;
    }

    private void UpdateMoveDirection()
    {
        float horizontal = GameManager.Instance.Input_GetAxis_Player("Horizontal");
        float vertical = GameManager.Instance.Input_GetAxis_Player("Vertical");

        Vector3 cameraForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z);

        moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
    }

    private void UpdateSpeed()
    {
        if (playerBase.isCrouch.Value) { speed = crouchSpeed; return; }
        if (playerBase.isSprint.Value) { speed = sprintSpeed; return; }
        speed = runSpeed;
    }

    public void UpdatePosition()
    {
        // 중력
        rigid.AddForce(Vector3.up * -gravity, ForceMode.Impulse);

        // 이동
        Vector3 position = transform.position + moveDirection.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(position);

        // 회전
        if (moveDirection != Vector3.zero)
        {
            // 이동 방향을 바라보도록 회전을 설정
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            // 캐릭터의 현재 회전을 목표 회전으로 부드럽게 전환
            rigid.rotation = Quaternion.Slerp(rigid.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // 점프
        if (playerBase.isJump.Value && jumpable)
        {
            jumpable = false;
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        if (playerBase.remainJumpDelayTime < 0f)
        {
            jumpable = true;
        }
    }
}