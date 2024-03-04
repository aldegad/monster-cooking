using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField]
    private int hp; // ���� ü�� 

    // �ʿ��� ���� ������Ʈ
    [SerializeField]
    private GameObject go_rock; // �Ϲ� ����

    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip striking_sound;
    [SerializeField]
    private AudioClip crash_sound;

    public void Mining()
    {
        audioSource.clip = striking_sound;
        audioSource.Play();

        hp--;

        if (hp <= 0)
        {
            Destruction();
        }
    }

    private void Destruction()
    {
        audioSource.clip = crash_sound;
        audioSource.Play();

        Destroy(go_rock, 1f);
    }
}
