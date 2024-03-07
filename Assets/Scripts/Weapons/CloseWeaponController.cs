using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CloseWeaponController : MonoBehaviour
{
    // ���� ������ Hand�� Ÿ�� ����
    [SerializeField]
    protected CloseWeapon currentCloseWeapon;

    // ������
    protected bool isAttack = false;
    protected bool isSwing = false; // ���� �ֵθ��� �ִ��� ����

    protected RaycastHit hitInfo;

    private Player thePlayer;

    private void Start()
    {

    }

    protected void TryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!isAttack)
            {
                // �ڷ�ƾ ����
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    protected IEnumerator AttackCoroutine()
    {
        isAttack = true;

        currentCloseWeapon.thePlayer.AttackAnimation(isAttack);

        yield return new WaitForSeconds(currentCloseWeapon.attackDelayA);
        isSwing = true;

        // ���� Ȱ��ȭ ����

        StartCoroutine(HitCoroutine());

        // ���� Ȱ��ȭ ����

        yield return new WaitForSeconds(currentCloseWeapon.attackDelayB);
        isSwing = false;

        yield return new WaitForSeconds(currentCloseWeapon.attackDelay - currentCloseWeapon.attackDelayA - currentCloseWeapon.attackDelayB); // �̹� ����Ѹ�ŭ ����

        isAttack = false;
    }

    protected abstract IEnumerator HitCoroutine();

    protected bool CheckObject()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentCloseWeapon.range))
        {
            return true;
        }

        return false;
    }

    public virtual void CloseWeaponChange(CloseWeapon _closeWeapon)
    {
        if (WeaponManager.currentWeapon != null)
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false);

            if (WeaponManager.currentWeapon.viewImage)
                WeaponManager.currentWeapon.viewImage.SetActive(false);
        }

        currentCloseWeapon = _closeWeapon;
        WeaponManager.currentWeapon = currentCloseWeapon;

        currentCloseWeapon.transform.localPosition = _closeWeapon.originPos;
        currentCloseWeapon.gameObject.SetActive(true);
    }
}
