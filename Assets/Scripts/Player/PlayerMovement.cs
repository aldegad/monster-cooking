using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Footsteps")]
    [Tooltip("Footstep source")]
    [SerializeField] private AudioSource footstepSource;

    [Tooltip("Distance for ground texture checker")]
    [SerializeField] private float groundCheckDistance = 1.0f;

    [Tooltip("Footsteps playing rate")]
    [SerializeField][Range(1f, 2f)] private float footstepRate = 1f;

    [Tooltip("Footstep rate when player running")]
    [SerializeField][Range(1f, 2f)] private float runningFootstepRate = 1.5f;

    [Tooltip("Add textures for this layer and add sounds to be played for this texture")]
    public List<GroundLayer> groundLayers = new List<GroundLayer>();

    private Rigidbody rigid;
    private Vector3 moveDirection;
    private float maxSpeed = 0f;

    //Private footstep system variables
    private Terrain _terrain;
    private TerrainData _terrainData;
    private TerrainLayer[] _terrainLayers;
    private AudioClip _previousClip;
    private Texture2D _currentTexture;
    private RaycastHit _groundHit;
    private float _nextFootstep;

    public override void OnNetworkSpawn()
    {
        rigid = GetComponent<Rigidbody>();
        maxSpeed = walkSpeed;

        if (!IsOwner) { return; }

        initializePosition();
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        if (!GameManager.Instance.GetPlayer(OwnerClientId).IsPlayerReady()) { return; }

        GetTerrainData();

        UpdateMoveDirection();
    }
    private void FixedUpdate()
    {
        // update footstep
        GroundChecker();

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

        updatePositionClientRpc(moveDirection);
    }

    [ClientRpc]
    private void updatePositionClientRpc(Vector3 moveDirection)
    {
        // 이거 현재는 순전히 footstep사운드 재생하기 위해서만 필요한건데... 이동과 풋스텝 사운드를 분리할까?
        FootStepChecker(moveDirection);
    }

    //Getting all terrain data for footstep system
    private void GetTerrainData()
    {
        if (SceneManager.GetActiveScene().name == "GameScene" && Terrain.activeTerrain)
        {
            if (_terrain != null) return;

            _terrain = Terrain.activeTerrain;
            _terrainData = _terrain.terrainData;
            _terrainLayers = _terrain.terrainData.terrainLayers;
        }
    }

    //Check where the controller is now and identify the texture of the component
    private void GroundChecker()
    {
        if (_terrainLayers == null) return;

        Ray checkerRay = new Ray(transform.position + (Vector3.up * 0.1f), Vector3.down);

        if (Physics.Raycast(checkerRay, out _groundHit, groundCheckDistance))
        {
            if (_groundHit.collider.GetComponent<Terrain>())
            {
                _currentTexture = _terrainLayers[GetTerrainTexture(transform.position)].diffuseTexture;
            }
            if (_groundHit.collider.GetComponent<Renderer>())
            {
                _currentTexture = GetRendererTexture();
            }
        }
    }

    private void FootStepChecker(Vector3 moveDirection)
    {
        if (moveDirection != Vector3.zero)
        {
            // 여기에 나중에 러닝체크도 넣어야 함. 현재 달리는거 없음.
            // float currentFootstepRate = (_isRunning ? runningFootstepRate : footstepRate);
            float currentFootstepRate = footstepRate;

            if (_nextFootstep >= 100f)
            {
                {
                    PlayFootstep();
                    _nextFootstep = 0;
                }
            }
            _nextFootstep += (currentFootstepRate * walkSpeed);
        }
    }

    //Play a footstep sound depending on the specific texture
    private void PlayFootstep()
    {
        for (int i = 0; i < groundLayers.Count; i++)
        {
            for (int k = 0; k < groundLayers[i].groundTextures.Length; k++)
            {
                if (_currentTexture == groundLayers[i].groundTextures[k])
                {
                    footstepSource.PlayOneShot(RandomClip(groundLayers[i].footstepSounds));
                }
            }
        }
    }

    //Return an array of textures depending on location of the controller on terrain
    private float[] GetTerrainTexturesArray(Vector3 controllerPosition)
    {
        _terrain = Terrain.activeTerrain;
        _terrainData = _terrain.terrainData;
        Vector3 terrainPosition = _terrain.transform.position;

        int positionX = (int)(((controllerPosition.x - terrainPosition.x) / _terrainData.size.x) * _terrainData.alphamapWidth);
        int positionZ = (int)(((controllerPosition.z - terrainPosition.z) / _terrainData.size.z) * _terrainData.alphamapHeight);

        float[,,] layerData = _terrainData.GetAlphamaps(positionX, positionZ, 1, 1);

        float[] texturesArray = new float[layerData.GetUpperBound(2) + 1];
        for (int n = 0; n < texturesArray.Length; ++n)
        {
            texturesArray[n] = layerData[0, 0, n];
        }
        return texturesArray;
    }

    //Returns the zero index of the prevailing texture based on the controller location on terrain
    private int GetTerrainTexture(Vector3 controllerPosition)
    {
        float[] array = GetTerrainTexturesArray(controllerPosition);
        float maxArray = 0;
        int maxArrayIndex = 0;

        for (int n = 0; n < array.Length; ++n)
        {

            if (array[n] > maxArray)
            {
                maxArrayIndex = n;
                maxArray = array[n];
            }
        }
        return maxArrayIndex;
    }

    //Returns the current main texture of renderer where the controller is located now
    private Texture2D GetRendererTexture()
    {
        Texture2D texture;
        texture = (Texture2D)_groundHit.collider.gameObject.GetComponent<Renderer>().material.mainTexture;
        return texture;
    }

    //Returns an audio clip from an array, prevents a newly played clip from being repeated and randomize pitch
    private AudioClip RandomClip(AudioClip[] clips)
    {
        int attempts = 2;
        footstepSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

        AudioClip selectedClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        while (selectedClip == _previousClip && attempts > 0)
        {
            selectedClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            attempts--;
        }
        _previousClip = selectedClip;
        return selectedClip;
    }
}

[Serializable]
public class GroundLayer
{
    public string layerName;
    public Texture2D[] groundTextures;
    public AudioClip[] footstepSounds;
}