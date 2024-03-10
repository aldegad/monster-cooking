using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class DebugCharacterChange : NetworkBehaviour
{
    [SerializeField] private GameObject debugCharacterChangeButtonContainer;
    [SerializeField] private Button debugCharacterChangeButton;
    private void Start()
    {
        Hide();

        List<Character> characters = GameManager.Instance.CharacterDatabase.GetCharacters();

        characters.ForEach(character => {
            Button buttonInstance = Instantiate(debugCharacterChangeButton, debugCharacterChangeButtonContainer.transform);

            buttonInstance.GetComponentInChildren<TMP_Text>().text = character.CharacterName;
            buttonInstance.onClick.AddListener(() =>
            {
                GameManager.Instance.UpdateCharacter(NetworkManager.Singleton.LocalClientId, character.CharacterId);
            });

            
        });
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    { 
        gameObject.SetActive(false);
    }
}
