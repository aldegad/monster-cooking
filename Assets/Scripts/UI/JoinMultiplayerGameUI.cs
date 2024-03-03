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

    public event UnityEngine.Events.UnityAction<string> OnJoinCodeEntered;

    private void Awake()
    {
        Hide();

        cancelJoinButton.onClick.AddListener(() =>
        {
            OnJoinCodeEntered?.Invoke(null);
            Hide();
        });

        submitJoinButton.onClick.AddListener(() =>
        {
            if (enterJoinCodeInput.text != "")
            {
                OnJoinCodeEntered?.Invoke(enterJoinCodeInput.text);
                Hide();
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
