using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
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

    public List<CharacterDefinition> characterDefinitionList;
    public List<WeaponDefinition> weaponDefinitionList;
    public List<AbilityDefinition[]> abilityDefinitionList;
    
    private List<CharacterDefinition> allyCharacters;
    private List<CharacterDefinition> enemyCharacters;
    

    private byte pickingCounter = 0;
    private void Awake()
    {
        
    }
    
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (pickingCounter < 2)
        {
            characterDefinitionList.Add(obj.GetComponent<CharacterDefinition>());
            obj.SetParent(canvas, false);
            pickingCounter++;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        obj.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        StartCoroutine(enlarge());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        obj.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        StartCoroutine(shrink());
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator enlarge()
    {
        float i = 0.015f;
        while (obj.localScale.x < 0.02f)
        {
            i += 0.001f;
            obj.localScale = new Vector3(i, i, i);
            yield return new WaitForSeconds(0.01f);
        }
    }
    
    public IEnumerator shrink(){
        float i = 0.02f;
        while (obj.localScale.x > 0.015f)
        {
            i -= 0.001f;
            obj.localScale = new Vector3(i, i, i);
            yield return new WaitForSeconds(0.01f);
        }
    }
}
