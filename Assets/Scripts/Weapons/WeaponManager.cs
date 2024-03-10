using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // ���� �ߺ� ��ü ���� ����
    public static bool isChangeWeapon = false;

    // ���� ����
    public static CloseWeapon currentWeapon;

    // ���� ������ Ÿ��
    [SerializeField]
    public string currentWeaponType;

    [SerializeField]
    private float changeWeaponDelayTime; // ���� ��ü �����̽ð�
    [SerializeField]
    private float changeWeaponEndDelayTime; // ���� ��ü�� ������ ��������

    // ���� ������ ���� ����
    [SerializeField]
    private CloseWeapon[] hands; // ���
    [SerializeField]
    private CloseWeapon[] pickaxes; // ���

    // ���� �������� ���� ���� ������ �����ϵ��� ��
    private Dictionary<string, CloseWeapon> handDictionary = new Dictionary<string, CloseWeapon>();
    private Dictionary<string, CloseWeapon> pickaxeDictionary = new Dictionary<string, CloseWeapon>();

    // �ʿ��� ������Ʈ
    [SerializeField]
    private HandController theHandController;
    [SerializeField]
    private PickaxeController thePickaxeController;

    private Player thePlayer;
    Test canvasComponent;

    // Start is called before the first frame update
    void Start()
    {
        // DUmp
        canvasComponent = GameObject.FindObjectOfType<Test>();
        // DUmp

        thePlayer = GetComponentInParent<Player>();
        thePlayer.anim.runtimeAnimatorController = currentWeapon.anim.runtimeAnimatorController;

        for (int i = 0; i < pickaxes.Length; i++)
        {
            handDictionary.Add(hands[i].closeWeaponName, hands[i]);
        }

        for (int i = 0; i < pickaxes.Length; i++)
        {
            pickaxeDictionary.Add(pickaxes[i].closeWeaponName, pickaxes[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isChangeWeapon)
        {
            // ���ⱳü ����
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                StartCoroutine(ChangeWeaponCoroutine("HAND", "HAND"));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                StartCoroutine(ChangeWeaponCoroutine("PICKAXE", "PICKAXE"));
            }
        }
    }

    public IEnumerator ChangeWeaponCoroutine(string _type, string _name)
    {
        isChangeWeapon = true;

        //currentWeaponAnim.SetTrigger("Weapon_Out");

        yield return new WaitForSeconds(changeWeaponDelayTime);

        CancelPreWeaponAction();
        WeaponChange(_type, _name);

        if(currentWeapon.viewImage)
            currentWeapon.viewImage.SetActive(true);

        thePlayer.anim.runtimeAnimatorController = currentWeapon.anim.runtimeAnimatorController;

        yield return new WaitForSeconds(changeWeaponEndDelayTime);

        currentWeaponType = _type;
        isChangeWeapon = false;

        if(canvasComponent)
        {
            canvasComponent.CheckTool(_name);
        }
    }

    private void CancelPreWeaponAction()
    {
        switch (currentWeaponType)
        {
            case "HAND":
                HandController.isActivate = false;
                break;
            case "PICKAXE":
                PickaxeController.isActivate = false;
                break;
        }
    }

    private void WeaponChange(string _type, string _name)
    {

        if (_type == "HAND")
        {
            theHandController.CloseWeaponChange(handDictionary[_name]);
        }
        else if (_type == "PICKAXE")
        {
            thePickaxeController.CloseWeaponChange(pickaxeDictionary[_name]);
        }
    }

    public RuntimeAnimatorController GetCurrentAnimController()
    {
        return currentWeapon.anim.runtimeAnimatorController;
    }
}
