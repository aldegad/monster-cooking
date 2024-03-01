using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using TMPro;

public class ConsoleUI : MonoBehaviour
{
    [SerializeField] private TMP_Text consoleText; // Inspector���� �Ҵ�
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
        // ���� �����忡�� �α� �޽��� ó��
        while (logMessages.TryDequeue(out string log))
        {
            consoleText.text += log + "\n";
        }
    }*/

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // �α� Ÿ�Կ� ���� ������ ����
        string color = "white"; // �⺻ ����
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

        // �ʿ��� ��� ���� Ʈ���̽��� �߰��� �� �ֽ��ϴ�:
        // consoleText.text += stackTrace + "\n";
    }
}
