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

    // 움직임 체크 변수
    private Vector3 lastPos;

    // 상태 변수
    private bool isWalk = false;
    private bool isRun = false;

    // 스피드 조정 변수
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
        // 카메라는 각자가 각자거를 쓴다. 네트워크로 동기화 할 필요 없음
        InitializeCamera();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        TryRun(); // Move 위에 위치 
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

        // 이동 시작 위치 설정
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
    private void HandleMovementClientRpc(Vector3 inputVector)
    {
        // 각 클라이언트의 같은 player 오브젝트에 뿌려준다!(맞을걸? 테스트해보자)
        if (inputVector != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, inputVector.normalized, rotationSpeed * Time.fixedDeltaTime);
        }

        rigid.MovePosition(transform.position + inputVector.normalized * movementSpeed * Time.fixedDeltaTime);
    }

    // 움직임 체크 후, 애니메이션
    private void MoveCheck()
    {
        // 여유값을 줌, 경사면이 있으면 살짝 미끄러질 수 있어서 걷고있다고 판단할 수 있기때문
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

    // 달리기 시도
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

    // 달리기 실행
    private void Running()
    {
        isRun = true;
        animator.SetBool("Running", isRun);
        applySpeed = runSpeed;
    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        animator.SetBool("Running", isRun);
        applySpeed = walkSpeed;
    }
}