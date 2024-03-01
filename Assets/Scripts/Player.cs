using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class Player : NetworkBehaviour
{
    private float movementSpeed = 10f;
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
        if (!IsOwner) {
            return;
        }

        InitializeCamera();
        AdjustInputForCameraDirection();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void InitializeCamera()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GameObject.Find("PlayerCamera").GetComponent<CinemachineFreeLook>();
            virtualCamera.Follow = gameObject.transform;
            virtualCamera.LookAt = gameObject.transform;
        }
    }

    private void AdjustInputForCameraDirection() {
        // 키 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);

        // 카메라 방향에 따른 입력 벡터 조정
        if (inputDirection != Vector3.zero)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            // 카메라의 y축 회전만 고려
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // 입력 벡터를 카메라 방향으로 변환
            inputVector = (cameraForward * vertical + cameraRight * horizontal).normalized;
        }
        else 
        {
            inputVector = inputDirection.normalized;
        }
    }

    private void HandleMovement() {
        HandleMovementServerRpc(inputVector);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleMovementServerRpc(Vector3 inputVector) {
        HandleMovementClientRpc(inputVector);
    }

    [ClientRpc]
    private void HandleMovementClientRpc(Vector3 inputVector) {
        // change direction & movement
        if (inputVector != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, inputVector.normalized, rotationSpeed * Time.fixedDeltaTime);
        }

        rigid.MovePosition(transform.position + inputVector.normalized * movementSpeed * Time.fixedDeltaTime);
    }
}