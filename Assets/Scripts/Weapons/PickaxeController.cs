using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PickaxeController : CloseWeaponController
{
    // Ȱ��ȭ ���� 
    public static bool isActivate = false;

    void Update()
    {
        if (isActivate)
        {
            TryAttack();
        }
    }

    protected override IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            // �浹����
            if (CheckObject())
            {
                if (hitInfo.transform.tag == "Rock")
                {
                    hitInfo.transform.GetComponent<Rock>().Mining();
                }

                isSwing = false;
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