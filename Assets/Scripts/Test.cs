using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    // �Ѿ� ���� �ݿ�
    [SerializeField]
    private TextMeshProUGUI wolk_tool;

    public void CheckTool(string tool_name)
    {
        wolk_tool.text = tool_name;
    }
}
