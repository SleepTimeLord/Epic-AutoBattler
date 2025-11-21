using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// this is literally just a placeholder the components of a card
public class CardSetter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI characterDescriptionText;
    public Image uniqueSkillPlaceholder;
    public Image regularSkillPlaceholder;
    public TextMeshProUGUI characterCost;
    public TextMeshProUGUI characterHealth;
    public Image swordIcon;
    public string instanceID;

    private void Awake()
    {
    }
    public void OnPointerClick(PointerEventData eventData)
    { 
        if (!SummonManager.Instance.isPlacingCard && !SummonManager.Instance.IsCardOnBoard(gameObject.GetComponent<CardSetter>())) {
            StartCoroutine(SummonManager.Instance.SummonCharacter(instanceID));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // make this bigger
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // go back to normal
    }
}