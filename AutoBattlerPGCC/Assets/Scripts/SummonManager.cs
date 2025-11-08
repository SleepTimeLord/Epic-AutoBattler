using System.Collections;
using UnityEngine;

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [Header("Spawn for character and character card")]
    public Transform spawnPosition;
    public RectTransform spawnPositionRect;
    [Header("Card Container")]
    public GameObject cardContainer;
    public Canvas canvas;
    private int cardInitialPosition;
    private RectTransform currentSummoned;
    private string currentSummonedID;

    [Header("Lerp Handler")]
    public float lerpSpeed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SummonCharacter(string instanceID)
    {
        if (currentSummoned == null)
        {
            SetCurrentSummon(instanceID);
            StartCoroutine(LerpToPosition(currentSummoned, spawnPosition));
            CharacterManager.Instance.SpawnCharacter(instanceID, spawnPosition.position, CharacterType.Ally);
        }
        else 
        {
            DesummonCharacter(currentSummonedID);
            SetCurrentSummon(instanceID);
            StartCoroutine(LerpToPosition(currentSummoned, spawnPosition));
        }
    }

    public void DesummonCharacter(string instanceID)
    {
        /*if (currentSummoned == null) { Debug.LogError("No Card To Desummon."); return; }

        currentSummoned.SetParent(cardContainer.transform);
        currentSummoned.SetCu
        currentSummoned = null;*/
        
        // goes back to the parent
        CharacterManager.Instance.DespawnCharacter(instanceID);
        currentSummoned.SetParent(cardContainer.transform, false);
        currentSummoned.SetSiblingIndex(cardInitialPosition);
        currentSummoned = null;
        currentSummonedID = null;
    }

    private void SetCurrentSummon(string instanceID) 
    {
        if (cardContainer.transform.childCount == 0) { Debug.Log("No children"); return; }
        foreach (RectTransform card in cardContainer.transform)
        {
            CardSetter cardInfo = card.GetComponent<CardSetter>();
            if (cardInfo == null)
            {
                Debug.LogError(card + " Doesn't have a CardSetter Script");
            }
            if (cardInfo.instanceID == instanceID)
            {
                currentSummoned = card;
                currentSummonedID = cardInfo.instanceID;
                currentSummoned.SetParent(canvas.transform);
                cardInitialPosition = currentSummoned.GetSiblingIndex();
                break;
            }
        }
    }

    private IEnumerator LerpToPosition(RectTransform target, Transform targetPosition)
    {
        if (target == null || targetPosition == null)
        {
            Debug.LogError("Target or TargetPosition is null.");
            yield break;
        }

        Vector3 startPos = target.position;
        Quaternion startRot = target.rotation;

        Vector3 targetPos = targetPosition.position;
        Quaternion targetRot = targetPosition.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * lerpSpeed;

            target.position = Vector3.Lerp(startPos, targetPos, t);
            target.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        target.position = targetPos;
        target.rotation = targetRot;
    }
}
