using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

public class Player : NetworkBehaviour
{
    [SerializeField] 
    private Vector3 startPosition;
    [SerializeField]
    private Animator animator;

    private float movementSpeed = 5f;
    private float rotationSpeed = 10f;

    private Rigidbody rigid;
    private CinemachineFreeLook virtualCamera;

    private Vector3 inputVector;

    // ������ üũ ����
    private Vector3 lastPos;

    // ���� ����
    private bool isWalk = false;
    private bool isRun = false;

    // ���ǵ� ���� ����
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;


    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        transform.position = startPosition;
    }

    private void Start()
    {
        applySpeed = walkSpeed;

        if (!IsOwner)
        {
            return;
        }
        //InitializePlayer();
        // ī�޶�� ���ڰ� ���ڰŸ� ����. ��Ʈ��ũ�� ����ȭ �� �ʿ� ����
        InitializeCamera();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        TryRun(); // Move ���� ��ġ 
        inputVector = AdjustInputForCameraDirection();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) 
        {
            return;
        }

        HandleMovement(inputVector);
        MoveCheck();
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
        GameObject picoChan = transform.Find("PicoChan").gameObject;
        GameObject amy = transform.Find("Amy").gameObject;

        if (IsOwner && !IsHost)
        {
            picoChan.SetActive(false);
            amy.SetActive(true);
        }

        // �̵� ���� ��ġ ����
        if (transform.position == Vector3.zero)
        {
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
        // Ű �Է�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // ī�޶��� y�� ȸ���� ���
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // �Է� ���͸� ī�޶� �������� ��ȯ
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
    private void HandleMovementClientRpc(Vector3 inputVector)
    {
        // �� Ŭ���̾�Ʈ�� ���� player ������Ʈ�� �ѷ��ش�!(������? �׽�Ʈ�غ���)
        if (inputVector != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, inputVector.normalized, rotationSpeed * Time.fixedDeltaTime);
        }

        rigid.MovePosition(transform.position + inputVector.normalized * movementSpeed * Time.fixedDeltaTime);
    }

    // ������ üũ ��, �ִϸ��̼�
    private void MoveCheck()
    {
        // �������� ��, ������ ������ ��¦ �̲����� �� �־ �Ȱ��ִٰ� �Ǵ��� �� �ֱ⶧��
        if (Vector3.Distance(lastPos, transform.position) >= 0.02f)
        {
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }

        animator.SetBool("Walking", isWalk);
        lastPos = transform.position;
    }

    // �޸��� �õ�
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    // �޸��� ����
    private void Running()
    {
        isRun = true;
        animator.SetBool("Running", isRun);
        applySpeed = runSpeed;
    }

    // �޸��� ���
    private void RunningCancel()
    {
        isRun = false;
        animator.SetBool("Running", isRun);
        applySpeed = walkSpeed;
    }
}