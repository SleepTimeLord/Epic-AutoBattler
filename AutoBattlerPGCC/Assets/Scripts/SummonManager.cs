using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SummonManager : MonoBehaviour
{
    public GameObject placeHolder;
    public LayoutElement placeHolderLayout;

    public GameObject enemyPlaceHolder;
    public LayoutElement enemyPlaceHolderLayout;
    public static SummonManager Instance { get; private set; }

    public bool isPlacingCard = false;
    public bool isPlacingEnemyCard = false;
    [Header("Spawn for character and character card")]
    public Transform spawnPosition;
    public Transform enemyPosition;
    [Header("Card Container")]
    public GameObject cardContainer;
    public GameObject enemyContainer;
    public Canvas canvas;

    private RectTransform currentSummoned;
    private string currentSummonedID;

    private RectTransform currentEnemySummoned;
    private string currentEnemySummonedID;

    private int lastIndex;
    private int enemyLastIndex;

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
            return;
        }
    }

    // gets card from deck
    public CardSetter GetCardFromDeck(int placement, CharacterType characterType)
    {
        if (characterType == CharacterType.Ally)
        {
            CardSetter card = cardContainer.transform.GetChild(placement).GetComponent<CardSetter>();
            if (card == null)
            {
                Debug.LogWarning("Card not not found");
                return null;
            }
            return card;
        }
        else
        {
            CardSetter card = enemyContainer.transform.GetChild(placement).GetComponent<CardSetter>();
            if (card == null)
            {
                Debug.LogWarning("Card not not found");
                return null;
            }
            return card;
        }
    }

    // spawns character from hand to board
    public IEnumerator SummonCharacter(string instanceID)
    {
        CharacterCreate character = CharacterManager.Instance.GetCharacter(instanceID, CharacterType.Ally);
        CardSetter card = character.characterCard.GetComponent<CardSetter>();

        if (currentSummoned == null)
        {
            if (card != null)
            {
                SetCurrentSummon(instanceID);
                // wait for character to spawn then lerp
                yield return StartCoroutine(LerpToPosition(currentSummoned, spawnPosition, CharacterType.Ally));
                CharacterManager.Instance.SpawnCharacter(instanceID, spawnPosition.position, CharacterType.Ally);
            }
            else
            {
                Debug.Log("cant find card");
            }
        }
        else
        {
            // finishes desummoning character before setingcurrentsummon
            StartCoroutine(SummonNewCard(currentSummonedID, instanceID, CharacterType.Ally));
        }
    }

    public IEnumerator SummonEnemyCharacter(string instanceID)
    {
        CharacterCreate character = CharacterManager.Instance.GetCharacter(instanceID, CharacterType.Enemy);
        CardSetter card = character.characterCard.GetComponent<CardSetter>();
        if (currentEnemySummoned == null)
        {
            if (card != null)
            {
                SetCurrentEnemySummon(instanceID);
                yield return StartCoroutine(LerpToPosition(currentEnemySummoned, enemyPosition, CharacterType.Enemy));
                CharacterManager.Instance.SpawnCharacter(instanceID, enemyPosition.position, CharacterType.Enemy);
            }
            else
            {
                Debug.Log("cant find card");
            }
        }
        else
        {
            StartCoroutine(SummonNewCard(currentEnemySummonedID, instanceID, CharacterType.Enemy));
        }
    }

    // make a card back to deck method to break this up
    private IEnumerator SummonNewCard(string currentInstanceID, string newInstanceID, CharacterType characterType)
    {
        CharacterManager.Instance.DespawnCharacter(currentInstanceID, characterType);
        // waits to desummon card
        yield return StartCoroutine(DesummonCard(currentInstanceID, characterType));
        if (characterType == CharacterType.Ally)
        {
            // after it stops lerping sets placeholder to ignore the layout
            placeHolderLayout.ignoreLayout = true;
            // sets the card going back to hand to cardcontainer in the same spot
            currentSummoned.SetParent(cardContainer.transform, false);
            currentSummoned.SetSiblingIndex(lastIndex);
            // set new current summoned
            SetCurrentSummon(newInstanceID);

            // summon new card to board
            StartCoroutine(LerpToPosition(currentSummoned, spawnPosition, characterType));
            CharacterManager.Instance.SpawnCharacter(newInstanceID, spawnPosition.position, characterType);
        }
        else
        {
            // after it stops lerping sets placeholder to ignore the layout
            enemyPlaceHolderLayout.ignoreLayout = true;
            // sets the card going back to hand to cardcontainer in the same spot
            currentEnemySummoned.SetParent(enemyContainer.transform, false);
            currentEnemySummoned.SetSiblingIndex(enemyLastIndex);
            SetCurrentEnemySummon(newInstanceID);

            StartCoroutine(LerpToPosition(currentEnemySummoned, enemyPosition, characterType));
            CharacterManager.Instance.SpawnCharacter(newInstanceID, enemyPosition.position, characterType);
        }
    }

    // This allows SummonNewCard to still access currentSummoned after desummoning
    // DesummonCard now handles animation and reparenting but does NOT clear currentSummoned
    public IEnumerator DesummonCard(string instanceID, CharacterType characterType)
    {
        if (characterType == CharacterType.Ally)
        {
            lastIndex = cardContainer.transform.childCount;
            placeHolderLayout.ignoreLayout = false;
            placeHolder.transform.SetSiblingIndex(lastIndex);

            // update card before desummoning
            CharacterManager.Instance.UpdateCharacterCard(instanceID, currentSummoned.GetComponent<CardSetter>(), characterType);

            // lerp back to hand
            yield return StartCoroutine(LerpToPosition(currentSummoned, placeHolder.transform, characterType));

            // reparent the card back to cardContainer after lerp completes
            placeHolderLayout.ignoreLayout = true;
            currentSummoned.SetParent(cardContainer.transform, false);
            currentSummoned.SetSiblingIndex(lastIndex);

            // The caller (KillCharacter or SummonNewCard) decides when to clear currentsummoned
        }
        else
        {
            enemyLastIndex = enemyContainer.transform.childCount;
            enemyPlaceHolderLayout.ignoreLayout = false;
            enemyPlaceHolder.transform.SetSiblingIndex(enemyLastIndex);

            CharacterManager.Instance.UpdateCharacterCard(instanceID, currentEnemySummoned.GetComponent<CardSetter>(), characterType);
            yield return StartCoroutine(LerpToPosition(currentEnemySummoned, enemyPlaceHolder.transform, characterType));

            // same for enemy
            enemyPlaceHolderLayout.ignoreLayout = true;
            currentEnemySummoned.SetParent(enemyContainer.transform, false);
            currentEnemySummoned.SetSiblingIndex(enemyLastIndex);

        }
    }

    // clears the currentSummoned references
    public void ClearCurrentSummoned(CharacterType characterType)
    {
        if (characterType == CharacterType.Ally)
        {
            currentSummoned = null;
            currentSummonedID = null;
        }
        else
        {
            currentEnemySummoned = null;
            currentEnemySummonedID = null;
        }
    }

    // sets card to the currently summoned card
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
                currentSummoned = card;
                currentSummonedID = cardInfo.instanceID;

                currentSummoned.SetParent(canvas.transform);
                break;
            }
        }
    }

    private void SetCurrentEnemySummon(string instanceID)
    {
        if (enemyContainer.transform.childCount == 0) { Debug.Log("No children"); return; }

        foreach (RectTransform card in enemyContainer.transform)
        {
            CardSetter cardInfo = card.GetComponent<CardSetter>();
            if (cardInfo == null)
            {
                continue;
            }
            if (cardInfo.instanceID == instanceID)
            {
                currentEnemySummoned = card;
                currentEnemySummonedID = cardInfo.instanceID;

                currentEnemySummoned.SetParent(canvas.transform);
                break;
            }
        }
    }

    // lerps to board
    private IEnumerator LerpToPosition(Transform target, Transform targetPosition, CharacterType characterType)
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

        if (characterType == CharacterType.Ally)
        {
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
        else
        {
            float t = 0f;
            while (t < 1f)
            {
                isPlacingEnemyCard = true;
                t += Time.deltaTime * lerpSpeed;

                target.position = Vector3.Lerp(startPos, targetPos, t);
                target.rotation = Quaternion.Lerp(startRot, targetRot, t);
                target.localScale = Vector3.Lerp(startScale, targetScale, t);

                yield return null;
            }

            target.position = targetPos;
            target.rotation = targetRot;
            target.localScale = targetScale;
            isPlacingEnemyCard = false;
        }
    }

    public bool IsCardOnBoard(CardSetter card)
    {
        return card.transform.parent == canvas.transform;
    }
}