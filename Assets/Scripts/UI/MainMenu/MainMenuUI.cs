using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startSingleButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    [SerializeField] private JoinMultiplayerGameUI joinMultiplayerGameUI;

    private void Awake()
    {
        startSingleButton.onClick.AddListener(() =>
        {
            GameManager.Instance.StartSingle();
        });

        startHostButton.onClick.AddListener(() =>
        {
            GameManager.Instance.StartHost();
        });

        startClientButton.onClick.AddListener(() =>
        {
            joinMultiplayerGameUI.Show();
        });
    }
}
