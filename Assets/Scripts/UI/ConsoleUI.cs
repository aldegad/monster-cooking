using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using TMPro;

public class ConsoleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text consoleText; // Inspector에서 할당
    //private ConcurrentQueue<string> logMessages = new ConcurrentQueue<string>();

    private void Awake()
    {
        consoleText.text = "";
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    /*private void Update()
    {
        // 메인 스레드에서 로그 메시지 처리
        while (logMessages.TryDequeue(out string log))
        {
            consoleText.text += log + "\n";
        }
    }*/

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

        // 필요한 경우 스택 트레이스도 추가할 수 있습니다:
        // consoleText.text += stackTrace + "\n";
    }
}
