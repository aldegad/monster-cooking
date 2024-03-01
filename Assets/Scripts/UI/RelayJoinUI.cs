using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelayJoinUI : MonoBehaviour
{

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button cancelJoinButton;
    [SerializeField] private Button submitJoinButton;

    private UnityEngine.Events.UnityAction<string> joinCallback;

    private void Awake()
    {
        Hide();

        cancelJoinButton.onClick.AddListener(() =>
        {
            joinCallback?.Invoke(null);
            Hide();
        });

        submitJoinButton.onClick.AddListener(() =>
        {
            if (inputField.text != "") {
                joinCallback?.Invoke(inputField.text);
                Hide();
            }
        });
    }
    public void Show() { 
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void JoinCallback(UnityEngine.Events.UnityAction<string> callback) {
        joinCallback = callback;
    }
}
