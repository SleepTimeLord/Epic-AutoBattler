using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SummonManager : MonoBehaviour
{
    public GameObject placeHolder;
    public LayoutElement placeHolderLayout;
    public static SummonManager Instance { get; private set; }

    [HideInInspector]
    public bool isPlacingCard = false;
    [Header("Spawn for character and character card")]
    public Transform spawnPosition;
    [Header("Card Container")]
    public GameObject cardContainer;
    public Canvas canvas;

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
        CharacterCreate character = CharacterManager.Instance.GetCharacter(instanceID);
        CardSetter card = character.characterCard.GetComponent<CardSetter>();
        if (currentSummoned == null)
        {
            if (card != null) 
            {
                SetCurrentSummon(instanceID);
                StartCoroutine(LerpToPosition(currentSummoned, spawnPosition));
                CharacterManager.Instance.SpawnCharacter(instanceID, spawnPosition.position, CharacterType.Ally);
            }
            else 
            {
                Debug.Log("cant find card");
            }

        }
        else 
        {
            //DesummonCharacter(currentSummonedID);
            // finishes desummoning character before setingcurrentsummon
            StartCoroutine(SummonNewCard(currentSummonedID, instanceID));
        }
    }

    private IEnumerator SummonNewCard(string currentInstanceID, string newInstanceID) 
    {
        int lastIndex = cardContainer.transform.childCount;
        placeHolderLayout.ignoreLayout = false;
        placeHolder.transform.SetSiblingIndex(lastIndex);
        // despawns physical instance of character
        CharacterManager.Instance.DespawnCharacter(currentInstanceID);
        // updates the CardSetter to new stats
        CharacterManager.Instance.UpdateCharacterCard(currentInstanceID, currentSummoned.GetComponent<CardSetter>());
        // goes bakc to the intial position 
        yield return StartCoroutine(LerpToPosition(currentSummoned, placeHolder.transform));
        // after it stops lerping sets placeholder to ignore the layout
        placeHolderLayout.ignoreLayout = true;
        // sets the card going back to hand to cardcontainer in the same spot
        currentSummoned.SetParent(cardContainer.transform, false);
        currentSummoned.SetSiblingIndex(lastIndex);
        SetCurrentSummon(newInstanceID);

        StartCoroutine(LerpToPosition(currentSummoned, spawnPosition));
        CharacterManager.Instance.SpawnCharacter(newInstanceID, spawnPosition.position, CharacterType.Ally);
    }


    private void SetCurrentSummon(string instanceID)
    {
        if (cardContainer.transform.childCount == 0) { Debug.Log("No children"); return; }

        foreach (RectTransform card in cardContainer.transform)
        {
            CardSetter cardInfo = card.GetComponent<CardSetter>();
            // this also skips the placeholder
            if (cardInfo == null)
            {
                continue;
            }

            if (cardInfo.instanceID == instanceID)
            {
                // store BEFORE re-parenting
                currentSummoned = card;
                currentSummonedID = cardInfo.instanceID;

                currentSummoned.SetParent(canvas.transform);
                break;
            }
        }
    }

    // lerps to board
    private IEnumerator LerpToPosition(Transform target, Transform targetPosition)
    {
        if (target == null || targetPosition == null)
        {
            Debug.LogError("Target or TargetPosition is null.");
            yield break;
        }

        Vector3 startPos = target.position;
        Quaternion startRot = target.rotation;
        Vector3 startScale = target.localScale;

        Vector3 targetPos = targetPosition.position;
        Quaternion targetRot = targetPosition.rotation;
        Vector3 targetScale = targetPosition.localScale;

        float t = 0f;
        while (t < 1f)
        {
            isPlacingCard = true;
            t += Time.deltaTime * lerpSpeed;

            target.position = Vector3.Lerp(startPos, targetPos, t);
            target.rotation = Quaternion.Lerp(startRot, targetRot, t);
            target.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        target.position = targetPos;
        target.rotation = targetRot;
        target.localScale = targetScale;
        isPlacingCard = false;
    }

    public bool IsCardOnBoard(CardSetter card)
    {
        return card.transform.parent == canvas.transform;
    }
}