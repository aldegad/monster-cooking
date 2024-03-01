using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameCodeUI : MonoBehaviour
{

    private TMP_Text gameCodeText;

    private void Awake()
    {
        Hide();
        gameCodeText = transform.Find("GameCodeText").GetComponent<TMP_Text>();
    }

    public void Show() { 
        gameObject.SetActive(true);
    }

    public void Hide() { 
        gameObject.SetActive(false);
    }

    public void SetGameCodeText(string gameCode) {
        gameCodeText.text = gameCode;
    }
}
