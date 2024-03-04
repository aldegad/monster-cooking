using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
public class Character : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject introPrefab;
    [SerializeField] private NetworkObject gamePlayPrefab;

    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject IntroPrefab => introPrefab;
    public NetworkObject GamePlayPrefab => gamePlayPrefab;
}