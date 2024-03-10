using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Character/Character")]
public class Character : ScriptableObject
{
    [SerializeField] private int characterId;
    [SerializeField] private string charadterName;
    [SerializeField] private GameObject characterPrefab;

    public int CharacterId => characterId;
    public string CharacterName => charadterName;
    public GameObject CharacterPrefab => characterPrefab;
}
