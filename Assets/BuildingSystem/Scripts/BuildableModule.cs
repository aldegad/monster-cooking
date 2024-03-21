using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableModule : MonoBehaviour
{
    [SerializeField] private GameObject modelParent;

    public GameObject ModelParent => modelParent;
}
