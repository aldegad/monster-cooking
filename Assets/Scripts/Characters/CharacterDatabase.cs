using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Character/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private List<Character> characters;

    public List<Character> GetCharacters() => characters;
    public Character GetCharacter(int characterId)
    {
        return characters.Find(character => character.CharacterId == characterId);
    }
}
