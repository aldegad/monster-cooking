using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildableTab : MonoBehaviour
{
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private int groupIndex;
    [SerializeField] private string displayName;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetFields(BuildingManager buildingManager, int groupIndex, string displayName)
    { 
        this.buildingManager = buildingManager;
        this.groupIndex = groupIndex;
        this.displayName = displayName;

        gameObject.GetComponentInChildren<TMP_Text>().text = displayName;
    }

    public void Actiavate(bool active)
    {
        if (active)
        {
            Color activeColor = GetComponent<Image>().color;
            activeColor.a = 150f / 255f;
            GetComponent<Image>().color = activeColor;
        }
        else
        {
            Color deactiveColor = GetComponent<Image>().color;
            deactiveColor.a = 200f / 255f;
            GetComponent<Image>().color = deactiveColor;
        }
    }

    private void OnClick()
    {
        buildingManager.changeBuildableGroup(groupIndex);
    }
}
