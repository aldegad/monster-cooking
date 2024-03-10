using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class HandController : CloseWeaponController
{
    // Ȱ��ȭ ���� 
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
            // �浹����
            if (CheckObject())
            {
                if (hitInfo.transform.tag == "Tree")
                {
                    hitInfo.transform.GetComponent<Tree>().Mining();
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
