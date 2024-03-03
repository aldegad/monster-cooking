using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField]
    private int hp; // ���� ü�� 

    // �ʿ��� ���� ������Ʈ
    [SerializeField]
    private GameObject go_rock; // �Ϲ� ����

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
