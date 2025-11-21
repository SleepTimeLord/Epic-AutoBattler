using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillPicker : MonoBehaviour, IPointerClickHandler
{
    public AbilityDefinition ad;
    public CardPickerManager manager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ad != null && manager != null)
        {
            manager.OnAbilityPicked(ad);
            Destroy(gameObject);
        }
    }
}
