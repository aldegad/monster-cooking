using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using TMPro;

public class DebugConsole : MonoBehaviour
{
    [SerializeField] private TMP_Text consoleText; // Inspector에서 할당

    private void Awake()
    {
        consoleText.text = "";
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 로그 타입에 따라 색상을 결정
        string color = "white"; // 기본 색상
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                color = "red";
                break;
            case LogType.Warning:
                color = "yellow";
                break;
            case LogType.Log:
                color = "white";
                break;
        }

        string formattedMessage = $"<color={color}>{logString}</color>\n";
        consoleText.text += formattedMessage;
    }
}
