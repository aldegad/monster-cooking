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

    public override void OnNetworkSpawn()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        if (IsOwner)
        {
            initializePosition();
        }
    }

    private void initializePosition()
    {
        int playerIndex = GameManager.Instance.GetPlayerIndex(OwnerClientId);

        if (playerIndex == -1) { return; }

        if (GameManager.Instance.players[playerIndex].position != Vector3.zero)
        {
            Vector3 position = GameManager.Instance.players[playerIndex].position;
            Debug.Log($"PlayerCharacter initializePosition: owner {OwnerClientId}, playerIndex: {playerIndex} position: {position}");
            UpdatePosition(position);
        }
        else
        {
            Debug.Log($"PlayerCharacter initializePosition: owner {OwnerClientId}, playerIndex: {playerIndex} position(isSpawned): {spawnPoint.position}");
            UpdatePosition(spawnPoint.position);
        }
    }

    public void UpdatePosition(Vector3 position)
    {
        updatePositionServerRpc(position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void updatePositionServerRpc(Vector3 position)
    { 
        rigid.Move(position, Quaternion.identity);
    }
}