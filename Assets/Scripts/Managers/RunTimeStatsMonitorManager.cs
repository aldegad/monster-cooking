using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunTimeStatsMonitorManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
