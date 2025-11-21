using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardPicker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Transform canvas;
    public Transform obj;
    public TextMeshProUGUI characterDescriptionText;
    public Image uniqueSkillPlaceholder;
    public TextMeshProUGUI characterCost;
    public TextMeshProUGUI characterHealth;

    // Set this in inspector
    public CharacterDefinition characterDefinition;



    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CardPickerManager.Instance.picked)
        {
            CardPickerManager.Instance.picked = true;
            obj.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            StartCoroutine(Enlarge());

            // Notify the manager that this card was picked
            if (CardPickerManager.Instance != null)
            {
                CardPickerManager.Instance.OnCardPicked(this);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CardPickerManager.Instance.picked)
        {
            obj.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            StartCoroutine(Enlarge());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CardPickerManager.Instance.picked)
        {
            obj.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            StartCoroutine(Shrink());
        }
    }

    public IEnumerator Enlarge()
    {
        float i = 0.015f;
        while (obj.localScale.x < 0.02f)
        {
            i += 0.001f;
            obj.localScale = new Vector3(i, i, i);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public IEnumerator Shrink()
    {
        float i = 0.02f;
        while (obj.localScale.x > 0.015f)
        {
            i -= 0.001f;
            obj.localScale = new Vector3(i, i, i);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
