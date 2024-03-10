using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // 무기 중복 교체 실행 방지
    public static bool isChangeWeapon = false;

    // 현재 무기
    public static CloseWeapon currentWeapon;

    // 현재 무기의 타입
    [SerializeField]
    public string currentWeaponType;

    [SerializeField]
    private float changeWeaponDelayTime; // 무기 교체 딜레이시간
    [SerializeField]
    private float changeWeaponEndDelayTime; // 무기 교체가 완전히 끝난시점

    // 무기 종류들 전부 관리
    [SerializeField]
    private CloseWeapon[] hands; // 곡괭이
    [SerializeField]
    private CloseWeapon[] pickaxes; // 곡괭이

    // 관리 차원에서 쉽게 무기 접근이 가능하도록 함
    private Dictionary<string, CloseWeapon> handDictionary = new Dictionary<string, CloseWeapon>();
    private Dictionary<string, CloseWeapon> pickaxeDictionary = new Dictionary<string, CloseWeapon>();

    // 필요한 컴포넌트
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
            // 무기교체 실행
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
