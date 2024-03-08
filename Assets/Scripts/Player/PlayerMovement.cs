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

    public void UpdatePosition(Vector3 inputVector)
    {
        updatePositionServerRpc(inputVector);
    }

    [ServerRpc(RequireOwnership = false)]
    private void updatePositionServerRpc(Vector3 inputVector)
    {
        Vector3 position = transform.position + inputVector.normalized * maxSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(position);

        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);
        GameManager.Instance.players[playerIndex] = PlayerData.SetPosition(playerIndex, position);
    }
    private void UpdateMoveDirection()
    {
        // Ű �Է�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontal, 0, vertical);

        // Debug.Log(inputVector);

        /*

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // ī�޶��� y�� ȸ���� ���
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // �Է� ���͸� ī�޶� �������� ��ȯ
        inputVector = (cameraForward * vertical + cameraRight * horizontal).normalized;*/
    }
}