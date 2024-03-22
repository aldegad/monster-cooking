using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
    [SerializeField] private bool _isRun = false;
    [SerializeField] private bool _isSprint = false;
    [SerializeField] private bool _isCrouch = false;

    [SerializeField] private bool _isJump = false;
    [SerializeField] private float jumpDelayTime = 0.1f;
    [SerializeField] private float _remainJumpDelayTime = 0f;

    [SerializeField] private bool _isFall = false;
    [SerializeField] private bool _isGround = false;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private float fallDelayTime = 0.2f;
    [SerializeField] private float remainFallDelayTime = 0f;

    private Vector3 previousPosition;
    private Vector3 velocity;

    public NetworkVariable<bool> isRun = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isSprint = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isCrouch = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isJump = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isFall = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isGround = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public float remainJumpDelayTime { get { return _remainJumpDelayTime; } private set { _remainJumpDelayTime = value; }}

    private void Awake()
    {
        isRun.Value = _isRun;
        isSprint.Value = _isSprint;
        isCrouch.Value = _isCrouch;
        isJump.Value = _isJump;
        isFall.Value = _isFall;
        isGround.Value = _isGround;
    }
    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);

        if (!IsOwner) { return; }
        Initialize();
    }
    private void Update()
    {
        if (!IsOwner) { return; }
        UpdateVelocity();
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
        GameManager.Instance.gameState = GameState.Exploration;
    }

    private void UpdateState()
    {
        if (GameManager.Instance.Input_GetButton_Player("Horizontal") || GameManager.Instance.Input_GetButton_Player("Vertical"))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isRun.Value = false;
                isSprint.Value = true;
            }
            else
            {
                isRun.Value = true;
                isSprint.Value = false;
            }
        }
        else
        {
            isRun.Value = false;
            isSprint.Value = false;
        }

        if (GameManager.Instance.Input_GetKeyDown_Player(KeyCode.C))
        {
            isCrouch.Value = !isCrouch.Value;
        }

        UpdateJumpState();
        UpdateFallState();
        UpdateGroundState();

        // update
        ChangeCameraState();
    }

    private void ChangeCameraState()
    {
        if (isCrouch.Value)
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
        if (!isJump.Value && isGround.Value)
        {
            remainJumpDelayTime -= Time.deltaTime;
        }

        if (GameManager.Instance.Input_GetButton_Player("Jump") && isGround.Value && remainJumpDelayTime <= 0f)
        {
            isJump.Value = true;
        }

        if (isJump.Value)
        {
            remainJumpDelayTime = jumpDelayTime;
            remainFallDelayTime = 0f;
        }

        if (isJump.Value && isFall.Value)
        {
            isJump.Value = false;
        }
    }

    private void UpdateFallState()
    {
        if (isGround.Value) {
            remainFallDelayTime = fallDelayTime;
        }
        else {
            remainFallDelayTime -= Time.deltaTime;
        }

        if (velocity.y < 0 && remainFallDelayTime < 0f)
        {
            isFall.Value = true;
        }
        else
        {
            isFall.Value = false;
        }
    }

    private void UpdateGroundState()
    {
        Ray checkerRay = new Ray(transform.position + (Vector3.up * 0.1f), Vector3.down);

        if (Physics.Raycast(checkerRay, groundCheckDistance))
        {
            isGround.Value = true;
        }
        else
        {
            isGround.Value = false;
        }
    }
}