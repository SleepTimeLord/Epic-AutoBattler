using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickedWeapon : MonoBehaviour, IPointerClickHandler
{
    public WeaponDefinition wd;
    public CardPickerManager manager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (wd != null && manager != null)
        {
            manager.OnWeaponPicked(wd);
            Destroy(gameObject);
        }
    }
}