using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnEnemy();
    }

    void SpawnEnemy() 
    {
        // gets card from deck and summons them
        CardSetter card = SummonManager.Instance.GetCardFromDeck(1, CharacterType.Enemy);
        if (card != null)
        {
            StartCoroutine(SummonManager.Instance.SummonEnemyCharacter(card.instanceID));
        }
        else
        {
            Debug.Log("cant find card");
        }
    }
}
