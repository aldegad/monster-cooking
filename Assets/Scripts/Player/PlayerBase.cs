using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerBase : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private PlayerFollowCamera playerFollowCamera;

    [SerializeField] private GameObject follow;
    [SerializeField] private GameObject lookAt;
    
    [SerializeField] private Vector3 standFollow = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 crouchFollow = new Vector3(0f, 0f, -1f);

    [SerializeField] private Vector3 standLookAt = new Vector3(0f, 1.068f, 0f);
    [SerializeField] private Vector3 crouchLookAt = new Vector3(0f, 0.6f, 0f);

    [Header("State")]
    [SerializeField] public bool isRun = false;
    [SerializeField] public bool isSprint = false;
    [SerializeField] public bool isCrouch = false;

    [SerializeField] public bool isJump = false;
    [SerializeField] private float jumpDelayTime = 0.1f;
    [SerializeField] public float remainJumpDelayTime = 0f;

    [SerializeField] public bool isFall = false;
    [SerializeField] public bool isGround = false;
    [SerializeField] public float groundCheckDistance = 0.3f;
    [SerializeField] private float fallDelayTime = 0.2f;
    [SerializeField] private float remainFallDelayTime = 0f;

    private Vector3 previousPosition;
    private Vector3 velocity;
    
    

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log($"PlayerBase OnNetworkSpawn OwnerClientId: ${OwnerClientId} / isOwner: {IsOwner}");

        if (!IsOwner) { return; }
        Initialize();
    }
    private void Update()
    {
        UpdateVelocity();

        if (!IsOwner) { return; }
        UpdateState();
    }


    private void Initialize()
    {
        // cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // audio
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        foreach (var listener in listeners)
        {
            listener.enabled = false;
        }
        gameObject.GetComponent<AudioListener>().enabled = true;

        // camera
        PlayerFollowCamera playerFollowCameraInstance = Instantiate(playerFollowCamera);
        playerFollowCameraInstance.freeLookCam.Follow = follow.transform;
        playerFollowCameraInstance.freeLookCam.LookAt = lookAt.transform;

        // set to global
        GameManager.Instance.SetCamera(playerFollowCameraInstance);
        GameManager.Instance.GameState = GameState.Exploration;
    }

    private void UpdateState()
    {
        if (GameManager.Instance.GameState == GameState.Exploration && Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
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

        if (GameManager.Instance.GameState == GameState.Exploration && Input.GetKeyDown(KeyCode.C))
        {
            isCrouch = !isCrouch;
        }

        UpdateJumpState();
        UpdateFallState();
        UpdateGroundState();

        // update
        UpdateStateServerRpc(isRun, isSprint, isCrouch, isJump, isFall, isGround, remainJumpDelayTime, remainFallDelayTime);
        ChangeCameraState();
    }

    private void ChangeCameraState()
    {
        if (isCrouch)
        {
            follow.transform.localPosition = Vector3.Lerp(follow.transform.localPosition, crouchFollow, 10 * Time.deltaTime);
            lookAt.transform.localPosition = Vector3.Lerp(lookAt.transform.localPosition, crouchLookAt, 10 * Time.deltaTime);
        }
        else
        {
            follow.transform.localPosition = Vector3.Lerp(follow.transform.localPosition, standFollow, 10 * Time.deltaTime);
            lookAt.transform.localPosition = Vector3.Lerp(lookAt.transform.localPosition, standLookAt, 10 * Time.deltaTime);
        }
    }

    private void UpdateVelocity()
    {
        if (previousPosition == null) previousPosition = transform.position;

        velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
    }

    private void UpdateJumpState()
    {
        if (!isJump && isGround)
        {
            remainJumpDelayTime -= Time.deltaTime;
        }

        if (GameManager.Instance.GameState == GameState.Exploration && Input.GetButtonDown("Jump") && isGround && remainJumpDelayTime <= 0f)
        {
            isJump = true;
        }

        if (isJump)
        {
            remainJumpDelayTime = jumpDelayTime;
            remainFallDelayTime = 0f;
        }

        if (isJump && isFall)
        {
            isJump = false;
        }
    }

    private void UpdateFallState()
    {
        if (isGround) {
            remainFallDelayTime = fallDelayTime;
        }
        else {
            remainFallDelayTime -= Time.deltaTime;
        }

        if (velocity.y < 0 && remainFallDelayTime < 0f)
        {
            isFall = true;
        }
        else
        {
            isFall = false;
        }
    }

    private void UpdateGroundState()
    {
        Ray checkerRay = new Ray(transform.position + (Vector3.up * 0.1f), Vector3.down);

        if (Physics.Raycast(checkerRay, groundCheckDistance))
        {
            isGround = true;
        }
        else
        {
            isGround = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateStateServerRpc(bool isRun, bool isSprint, bool isCrouch, bool isJump, bool isFall, bool isGround, float remainJumpDelayTime, float remainFallDelayTime)
    {
        UpdateStateClientRpc(isRun, isSprint, isCrouch, isJump, isFall, isGround, remainJumpDelayTime, remainFallDelayTime);
    }

    [ClientRpc]
    private void UpdateStateClientRpc(bool isRun, bool isSprint, bool isCrouch, bool isJump, bool isFall, bool isGround, float remainJumpDelayTime, float remainFallDelayTime)
    {
        if (IsOwner) { return; }
        // 다른 사람들의 상태 업데이트
        this.isRun = isRun;
        this.isSprint = isSprint;
        this.isCrouch = isCrouch;
        this.isJump = isJump;
        this.isFall = isFall;
        this.isGround = isGround;
        this.remainJumpDelayTime = remainJumpDelayTime;
        this.remainFallDelayTime = remainFallDelayTime;
    }
}