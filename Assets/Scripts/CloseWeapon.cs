using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseWeapon : MonoBehaviour
{
    public string closeWeaponName; // 너클 or 맨손을 구분

    // 웨폰 유형
    public bool isHand;
    public bool isAxe;
    public bool isPickaxe;

    public float range; // 공격 범위
    public int damage; // 공격력
    public float workSpeed; // 작업 속도
    public float attackDelay; // 공격 딜레이
    public float attackDelayA; // 공격 활성화 시점 - 해당 시점에 닿은 오브젝트 데미지 들어감
    public float attackDelayB; // 공격 비활성화 시점 - 해당 시점에 닿은 오브젝트는 데미지 안들어감

    //public Animator anim; // 애니메이션
}
