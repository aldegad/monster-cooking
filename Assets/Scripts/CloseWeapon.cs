using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseWeapon : MonoBehaviour
{
    public string closeWeaponName; // ��Ŭ or �Ǽ��� ����

    // ���� ����
    public bool isHand;
    public bool isAxe;
    public bool isPickaxe;

    public float range; // ���� ����
    public int damage; // ���ݷ�
    public float workSpeed; // �۾� �ӵ�
    public float attackDelay; // ���� ������
    public float attackDelayA; // ���� Ȱ��ȭ ���� - �ش� ������ ���� ������Ʈ ������ ��
    public float attackDelayB; // ���� ��Ȱ��ȭ ���� - �ش� ������ ���� ������Ʈ�� ������ �ȵ�

    //public Animator anim; // �ִϸ��̼�
}
