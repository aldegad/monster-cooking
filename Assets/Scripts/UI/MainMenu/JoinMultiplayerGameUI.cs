using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JoinMultiplayerGameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField enterJoinCodeInput;
    [SerializeField] private Button cancelJoinButton;
    [SerializeField] private Button submitJoinButton;

    private void Awake()
    {
        cancelJoinButton.onClick.AddListener(() =>
        {
            Hide();
        });

        submitJoinButton.onClick.AddListener(async() =>
        {
            if (enterJoinCodeInput.text != "")
            {
                Hide();
                await GameManager.Instance.StartClient(enterJoinCodeInput.text);
            }
        });
    }

    private void OnEnable()
    {
        enterJoinCodeInput.text = "";
    }

    public void Show() 
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    { 
        gameObject.SetActive(false);
    }
}
