using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PickaxeController : CloseWeaponController
{
    // 활성화 여부 
    public static bool isActivate = true;

    void Update()
    {
        if (isActivate)
        {
            TryAttack();
        }
    }

    private void Start()
    {
        WeaponManager.currentWeapon = currentCloseWeapon;
    }

    protected override IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            // 충돌했음
            if (CheckObject())
            {
                if (hitInfo.transform.tag == "Rock")
                {
                    hitInfo.transform.GetComponent<Rock>().Mining();
                }

                isSwing = false;
                Debug.Log("Mining : " + hitInfo.transform.name);
            }

            yield return null;
        }
    }

    public override void CloseWeaponChange(CloseWeapon _closeWeapon)
    {
        base.CloseWeaponChange(_closeWeapon);

        isActivate = true;
    }
}
