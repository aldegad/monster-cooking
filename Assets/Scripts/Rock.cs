using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField]
    private int hp; // 바위 체력 

    // 필요한 게임 오브젝트
    [SerializeField]
    private GameObject go_rock; // 일반 바위

    public void Mining()
    {
        hp--;

        if(hp <= 0)
        {
            Destruction();
        }
    }

    private void Destruction()
    {
        Debug.Log("Rock!");
    }
}
