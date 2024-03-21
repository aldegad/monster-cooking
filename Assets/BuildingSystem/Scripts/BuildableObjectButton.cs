using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildableObjectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private BuildableObject buildableObject;
    [SerializeField] private int buildableGroupIndex;
    [SerializeField] private int buildableIndex;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetFields(BuildableObject buildableObject, int buildableGroupIndex, int buildableIndex)
    { 
        this.buildableObject = buildableObject;
        this.buildableGroupIndex = buildableGroupIndex;
        this.buildableIndex = buildableIndex;

        gameObject.GetComponentInChildren<RawImage>().texture = buildableObject.thumbnailImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("mouse enter");
        // 여기에 마우스가 버튼 위에 올라왔을 때 실행할 코드를 작성하세요.
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Debug.Log("mouse leave");
        // 여기에 마우스가 버튼을 벗어났을 때 실행할 코드를 작성하세요.
    }
    private void OnClick()
    {
        BuildingManager.Instance.ChangeBuildingTypeButton(buildableObject.buildType);
        BuildingManager.Instance.StartBuildingButton(buildableGroupIndex, buildableIndex);
    }
}
