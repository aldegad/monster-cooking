using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerBase : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineFreeLook playerFollowCamera;

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

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log($"PlayerBase OnNetworkSpawn OwnerClientId: ${OwnerClientId} / isOwner: {IsOwner}");

        if (!IsOwner) { return; }
        Initialize();
    }
    private void Update()
    {
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
        CinemachineFreeLook playerFollowCameraInstance = Instantiate(playerFollowCamera);
        playerFollowCameraInstance.Follow = follow.transform;
        playerFollowCameraInstance.LookAt = lookAt.transform;
    }

    private void UpdateState()
    {
        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
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

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouch = !isCrouch;
        }

        ChangeCameraState();
        UpdateStateServerRpc(isRun, isSprint, isCrouch);
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

    [ServerRpc(RequireOwnership = false)]
    private void UpdateStateServerRpc(bool isRun, bool isSprint, bool isCrouch)
    {
        UpdateStateClientRpc(isRun, isSprint, isCrouch);
    }

    [ClientRpc]
    private void UpdateStateClientRpc(bool isRun, bool isSprint, bool isCrouch)
    {
        if (IsOwner) { return; }
        // 다른 사람들의 상태 업데이트
        this.isRun = isRun;
        this.isSprint = isSprint;
        this.isCrouch = isCrouch;
    }
}