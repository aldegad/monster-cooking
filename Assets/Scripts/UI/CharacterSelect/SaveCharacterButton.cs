using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SaveCharacterButton : NetworkBehaviour
{
    [SerializeField] private GameObject characterSelectUI;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            ulong clientId = NetworkManager.Singleton.LocalClient.ClientId;
            // �ӽ� ĳ���� ���̵�
            int characterId = (int)NetworkManager.Singleton.LocalClient.ClientId + 1;
            // ĳ���� ���� �Ϸ�!
            GameManager.Instance.SetCharacter(clientId, characterId);
            GameManager.Instance.SpawnPlayer();
            Hide();
        });
    }

    private void Hide()
    {
        characterSelectUI.SetActive(false);
    }
}
