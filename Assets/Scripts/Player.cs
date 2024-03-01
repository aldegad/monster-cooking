using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Vector3 startPosition;

    private float movementSpeed = 5f;
    private float rotationSpeed = 10f;

    private Rigidbody rigid;
    private CinemachineFreeLook virtualCamera;

    private Vector3 inputVector;

    // Start is called before the first frame update
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        InitializePlayer();
        InitializeCamera();

        inputVector = AdjustInputForCameraDirection();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) 
        {
            return;
        }

        HandleMovement(inputVector);
    }

    private void InitializePlayer()
    {
        InitializePlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitializePlayerServerRpc()
    {
        InitializePlayerClientRpc();
    }
    
    [ClientRpc]
    private void InitializePlayerClientRpc()
    {
        if (transform.position == Vector3.zero)
        {
            if (IsHost)
            {
                transform.Find("PicoChan").gameObject.SetActive(true);
            }
            else
            {
                transform.Find("Amy").gameObject.SetActive(true);
            }

            rigid.MovePosition(startPosition);
        }
    }

    private void InitializeCamera()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineFreeLook>();
            virtualCamera.Follow = transform.Find("CameraFocus").transform;
            virtualCamera.LookAt = transform.Find("CameraFocus").transform;
        }
    }

    private Vector3 AdjustInputForCameraDirection() {
        // 키 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // 카메라의 y축 회전만 고려
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 입력 벡터를 카메라 방향으로 변환
        Vector3 inputVector = (cameraForward * vertical + cameraRight * horizontal).normalized;

        return inputVector;
    }

    private void HandleMovement(Vector3 inputVector) {
        HandleMovementServerRpc(inputVector);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleMovementServerRpc(Vector3 inputVector) {
        HandleMovementClientRpc(inputVector);
    }

    [ClientRpc]
    private void HandleMovementClientRpc(Vector3 inputVector) {
        if (inputVector != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, inputVector.normalized, rotationSpeed * Time.fixedDeltaTime);
        }

        rigid.MovePosition(transform.position + inputVector.normalized * movementSpeed * Time.fixedDeltaTime);
    }
}