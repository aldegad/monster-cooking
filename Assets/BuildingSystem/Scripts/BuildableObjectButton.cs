using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildableObjectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private BuildableModule buildableModule;
    [SerializeField] private int buildableGroupIndex;
    [SerializeField] private int buildableIndex;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetFields(BuildableModule buildableModule, int buildableGroupIndex, int buildableIndex)
    { 
        this.buildableModule = buildableModule;
        this.buildableGroupIndex = buildableGroupIndex;
        this.buildableIndex = buildableIndex;

        gameObject.GetComponentInChildren<RawImage>().texture = buildableModule.thumbnailImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("mouse enter");
        // ���⿡ ���콺�� ��ư ���� �ö���� �� ������ �ڵ带 �ۼ��ϼ���.
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("mouse leave");
        // ���⿡ ���콺�� ��ư�� ����� �� ������ �ڵ带 �ۼ��ϼ���.
    }
    private void OnClick()
    {
        BuildingManager.Instance.ChangeBuildingTypeButton(buildableModule.buildType);
        BuildingManager.Instance.StartBuildingButton(buildableGroupIndex, buildableIndex);
    }
}
