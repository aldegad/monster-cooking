using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildableObjectButton : MonoBehaviour
{
    [SerializeField] public BuildingManager buildingManager;
    [SerializeField] public SelectedBuildType selectedBuildType;
    [SerializeField] public int buildingIndex;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        buildingManager.changeBuildingTypeButton(selectedBuildType);
        buildingManager.startBuildingButton(buildingIndex);
    }
}
