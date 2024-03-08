using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rigid;
    private Animator anim;
    private Vector3 inputVector;

    public override void OnNetworkSpawn()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        if (!IsOwner) { return; }

        initializePosition();
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        if (!GameManager.Instance.GetPlayer(OwnerClientId).IsPlayerReady()) { return; }

        UpdateInputVecter();
    }
    private void FixedUpdate()
    {
        if (!IsOwner) { return; }
        if (!GameManager.Instance.GetPlayer(OwnerClientId).IsPlayerReady()) { return; }

        if (inputVector == Vector3.zero) { return; }

        UpdatePosition(inputVector);
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
        Vector3 position = transform.position + inputVector.normalized * walkSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(position);

        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);
        GameManager.Instance.players[playerIndex] = PlayerData.SetPosition(playerIndex, position);
    }
    private void UpdateInputVecter()
    {
        // 키 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);

        inputVector = inputDirection.normalized;

        // Debug.Log(inputVector);

        /*

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // 카메라의 y축 회전만 고려
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 입력 벡터를 카메라 방향으로 변환
        inputVector = (cameraForward * vertical + cameraRight * horizontal).normalized;*/
    }
}