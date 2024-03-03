using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Charater Database", menuName = "Characters/Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private Character[] characters = new Character[0];

    public Character[] GetAllCharacters() => characters;

    public Character GetCharacterById(int id)
    {
        foreach (Character character in characters)
        {
            if (character.Id == id)
            { 
                return character;
            }
        }

        return null;
    }

    public bool IsValidCharaterId(int id)
    {
        return characters.Any(character => character.Id == id);
    }
}
