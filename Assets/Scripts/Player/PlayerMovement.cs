using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rigid;
    private Animator anim;
    private Vector3 moveDirection;
    private float maxSpeed = 0f;

    public override void OnNetworkSpawn()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        maxSpeed = walkSpeed;

        if (!IsOwner) { return; }

        initializePosition();
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        if (!GameManager.Instance.GetPlayer(OwnerClientId).IsPlayerReady()) { return; }

        UpdateMoveDirection();
    }
    private void FixedUpdate()
    {
        if (!IsOwner) { return; }
        if (!GameManager.Instance.GetPlayer(OwnerClientId).IsPlayerReady()) { return; }

        if (moveDirection == Vector3.zero) { return; }

        UpdatePosition(moveDirection);
    }

    private void initializePosition()
    {
        initializePositionServerRpc();
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
    }

    private void UpdateMoveDirection()
    {
        // 키 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 cameraForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        Vector3 cameraRight = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z);

        moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
    }

    public void UpdatePosition(Vector3 moveDirection)
    {
        updatePositionServerRpc(moveDirection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void updatePositionServerRpc(Vector3 moveDirection)
    {
        Vector3 position = transform.position + moveDirection.normalized * maxSpeed * Time.fixedDeltaTime;

        if (moveDirection != Vector3.zero)
        {
            // 이동 방향을 바라보도록 회전을 설정
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            // 캐릭터의 현재 회전을 목표 회전으로 부드럽게 전환
            rigid.rotation = Quaternion.Slerp(rigid.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        rigid.MovePosition(position);

        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);
        GameManager.Instance.players[playerIndex] = PlayerData.SetPosition(playerIndex, position);
    }
}