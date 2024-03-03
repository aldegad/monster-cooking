using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] private Button button;

    private CharacterSelectDisplay characterSelect;
    public Character Character { get; private set; }

    public bool IsDisabled { get; private set; }

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => {
            SelectCharacter();
        });
    }

    public void SetCharacter(CharacterSelectDisplay characterSelect, Character character)
    {
        iconImage.sprite = character.Icon;
        characterName.text = character.DisplayName;

        this.characterSelect = characterSelect;

        Character = character;
    }

    public void SelectCharacter()
    {
        characterSelect.Select(Character);
    }

    public void SetDisabled()
    {
        IsDisabled = true;
        disabledOverlay.SetActive(true);
        button.interactable = false;
    }
}
