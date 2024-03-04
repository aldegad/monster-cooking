using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    [SerializeField] private JoinMultiplayerGameUI joinMultiplayerGameUI;

    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            ServerManager.Instance.StartHost();
        });

        startClientButton.onClick.AddListener(() =>
        {
            joinMultiplayerGameUI.Show();
        });
    }
}
