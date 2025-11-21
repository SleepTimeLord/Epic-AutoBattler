using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform enemyCardContainer;

    public IEnumerator SpawnEnemyRoutine()
    {
        CardSetter card = null;

        // keep trying until we get a valid card
        while (card == null)
        {
            // 1. Calculate deck size, excluding the placeholder card.
            // Assuming the placeholder is always the last element (index: deck.Count - 1).
            int deckSizeWithoutPlaceholder = enemyCardContainer.childCount - 1;

            if (deckSizeWithoutPlaceholder <= 0)
            {
                Debug.LogWarning("Enemy deck is empty or contains only the placeholder, cannot spawn a valid enemy.");
                yield break;
            }

            int randomIndex = UnityEngine.Random.Range(0, deckSizeWithoutPlaceholder);

            card = SummonManager.Instance.GetCardFromDeck(randomIndex, CharacterType.Enemy);

            if (card == null)
            {
                Debug.Log("Failed to get card (unexpected), retrying...");
                yield return null; 
            }
        }

        // once we have a valid card
        yield return StartCoroutine(SummonManager.Instance.SummonEnemyCharacter(card.instanceID));
    }
}
