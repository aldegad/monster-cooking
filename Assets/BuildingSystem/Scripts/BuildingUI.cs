using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingUI : MonoBehaviour
{
    [Header("Building UI")]
    [SerializeField] private GameObject buildableTabGroup;
    [SerializeField] private BuildableTab buildableTab;
    [SerializeField] private GameObject buildableObjectContainer;
    [SerializeField] private GameObject buildableObjectGrid;
    [SerializeField] private BuildableObjectButton buildableObjectButton;

    private List<BuildableTab> buildableTabInstances = new List<BuildableTab>();
    private List<GameObject> buildableObjectGridInstances = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < BuildingManager.Instance.BuildableGroups.Count; i++)
        {
            BuildableGroup buildableGroup = BuildingManager.Instance.BuildableGroups[i];
            // set tabs
            BuildableTab buildableTabInstance = Instantiate(buildableTab, buildableTabGroup.transform);
            buildableTabInstances.Add(buildableTabInstance);

            buildableTabInstance.SetFields(i, buildableGroup.buildableGroupName);
            if (i == 0) buildableTabInstance.Actiavate(true);
            else buildableTabInstance.Actiavate(false);

            // set tab grids
            GameObject buildableObjectGridInstance = Instantiate(buildableObjectGrid, buildableObjectContainer.transform);
            if (i != 0) buildableObjectGridInstance.SetActive(false);
            buildableObjectGridInstances.Add(buildableObjectGridInstance);

            for (int j = 0; j < buildableGroup.buildableObjects.Count; j++)
            {
                BuildableObject buildableObject = buildableGroup.buildableObjects[j];

                BuildableObjectButton buildableObjectButtonInstance = Instantiate(buildableObjectButton, buildableObjectGridInstance.transform);

                buildableObjectButtonInstance.SetFields(buildableObject, i, j);
            }
        }
    }

    public void ChangeBuildableGroup(int buildableGroupIndex)
    {
        // change tab
        buildableTabInstances[buildableGroupIndex].Actiavate(true);

        for (int i = 0; i < buildableTabInstances.Count; i++)
        {
            if (i != buildableGroupIndex)
            {
                buildableTabInstances[i].Actiavate(false);
            }
        }

        // change tab container
        buildableObjectGridInstances[buildableGroupIndex].SetActive(true);

        for (int i = 0; i < buildableObjectGridInstances.Count; i++)
        {
            if (i != buildableGroupIndex)
            {
                buildableObjectGridInstances[i].SetActive(false);
            }
        }
    }
}
