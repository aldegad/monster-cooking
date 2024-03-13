using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [Header("Movement")]
    [SerializeField] public float runSpeed = 5f;
    [SerializeField] public float sprintSpeed = 8f;
    [SerializeField] public float crouchSpeed = 3f;
    [SerializeField] public float rotationSpeed = 10f;

    private PlayerAnimation playerAnimation;
    private bool isNetworkSpawned = false;
    private bool isInitialized = false;
    private Rigidbody rigid;
    [HideInInspector]  public Vector3 moveDirection;
    private float speed = 0f;

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        rigid = GetComponent<Rigidbody>();
        rigid.useGravity = false;
        speed = runSpeed;
    }

    public override void OnNetworkSpawn()
    {
        isNetworkSpawned = true;
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        initializePosition();

        if (!isInitialized) { return; }
        UpdateSpeed();
        UpdateMoveDirection();
    }
    private void FixedUpdate()
    {
        if (!IsOwner) { return; }
        if (!isInitialized) { return; }

        UpdatePosition(moveDirection, speed);
    }

    private void initializePosition()
    {
        if (isInitialized) { return; }
        if (SceneManager.GetActiveScene().name == "GameScene" && isNetworkSpawned)
        {
            initializePositionServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void initializePositionServerRpc()
    {
        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);

        if (playerIndex == -1) { return; }

        Vector3 position;

        if (GameManager.Instance.players[playerIndex].position != Vector3.zero)
        {
            position = GameManager.Instance.players[playerIndex].position;
        }
        else
        {
            position = spawnPoint.position;
        }

        rigid.MovePosition(position);
        GameManager.Instance.players[playerIndex] = PlayerData.SetPosition(playerIndex, position);

        initializePositionClientRpc();
    }
    
    [ClientRpc]
    private void initializePositionClientRpc()
    {
        isInitialized = true;
        rigid.useGravity = true;
    }

    private void UpdateMoveDirection()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 cameraForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z);

        moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
    }

    private void UpdateSpeed()
    {
        if (playerAnimation.isCrouch) { speed = crouchSpeed; return; }
        if (playerAnimation.isSprint) { speed = sprintSpeed; return; }
        speed = runSpeed;
    }

    public void UpdatePosition(Vector3 moveDirection, float speed)
    {
        updatePositionServerRpc(moveDirection, speed);
    }

    [ServerRpc(RequireOwnership = false)]
    private void updatePositionServerRpc(Vector3 moveDirection, float speed)
    {
        Vector3 position = transform.position + moveDirection.normalized * speed * Time.fixedDeltaTime;

        if (moveDirection != Vector3.zero)
        {
            // �̵� ������ �ٶ󺸵��� ȸ���� ����
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            // ĳ������ ���� ȸ���� ��ǥ ȸ������ �ε巴�� ��ȯ
            rigid.rotation = Quaternion.Slerp(rigid.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        rigid.MovePosition(position);

        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);
        GameManager.Instance.players[playerIndex] = PlayerData.SetPosition(playerIndex, position);

        updatePositionClientRpc(moveDirection);
    }

    [ClientRpc]
    private void updatePositionClientRpc(Vector3 moveDirection)
    {
        // ���峪 �̷� ������ ������ ��ߵǰŵ�.
        this.moveDirection = moveDirection;
    }
}