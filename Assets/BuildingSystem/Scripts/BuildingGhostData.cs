using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGhostData
{
    public BuildableModule ghostModule;
    public bool isBuilding = false;
    public bool isDestroying = false;
    public bool isGhostValidPosition = false;
    public int currentBuildableGroupIndex = -1;
    public int currentBuildableIndex = -1;
    public Vector3 ghostPosition = Vector3.zero;
    public Quaternion ghostRotation = Quaternion.identity;

    public BuildingGhostData() {
        
    }
    public BuildingGhostData(bool isBuilding, bool isDestroying, bool isGhostValidPosition, int currentBuildableGroupIndex, int currentBuildableIndex, Vector3 ghostPosition, Quaternion ghostRotation)
    { 
        this.isBuilding = isBuilding;
        this.isDestroying = isDestroying;
        this.isGhostValidPosition = isGhostValidPosition;
        this.currentBuildableGroupIndex = currentBuildableGroupIndex;
        this.currentBuildableIndex = currentBuildableIndex;
        this.ghostPosition = ghostPosition;
        this.ghostRotation = ghostRotation;
    }
}