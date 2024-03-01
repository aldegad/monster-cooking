using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private RelayJoinUI relayJoinUI;
    [SerializeField] private GameCodeUI gameCodeUI;

    private void Awake()
    {
        startHostButton.onClick.AddListener(async () =>
        {
            string joinCode = await MultiGameManager.Instance.StartHostWithRelay();
            Debug.Log(joinCode);

            gameCodeUI.SetGameCodeText($"Game Code: {joinCode}");
            gameCodeUI.Show();

            Hide();
        });

        startClientButton.onClick.AddListener(() =>
        {
            relayJoinUI.Show();
            relayJoinUI.JoinCallback(async (joinCode) => {
                if (joinCode != null) {
                    bool isJoined = await MultiGameManager.Instance.StartClientWithRelay(joinCode);

                    gameCodeUI.SetGameCodeText($"Game Code: {joinCode}");
                    gameCodeUI.Show();

                    Hide();
                }
            });
        });
    }

    private void Hide() { 
        gameObject.SetActive(false);
    }
}
