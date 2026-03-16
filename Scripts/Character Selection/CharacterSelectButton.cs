using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private CharacterSelectUI characterSelect;
    private Character character;

    public void SetCharacter(CharacterSelectUI characterSelect, Character character)
    {
        iconImage.sprite = character.Icon;

        this.characterSelect = characterSelect;
        this.character = character;
    }

    public void SelectCharacter()
    {
        characterSelect.Select(character);
    }
}
