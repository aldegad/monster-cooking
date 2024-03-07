using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [SerializeField]
    private int hp; // 바위 체력 

    // 필요한 게임 오브젝트
    [SerializeField]
    private GameObject go_rock; // 일반 바위

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
