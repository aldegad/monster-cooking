using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    // ÃÑ¾Ë °¹¼ö ¹Ý¿µ
    [SerializeField]
    private TextMeshProUGUI wolk_tool;

    public void CheckTool(string tool_name)
    {
        wolk_tool.text = tool_name;
    }
}
