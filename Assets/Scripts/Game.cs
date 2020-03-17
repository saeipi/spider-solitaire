using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Difficulty
{
    Easy, Medium, Hard
}

public class Game : MonoBehaviour
{
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Positioner positioner;
    private List<Card>[] stacks;

    void Start()
    {
        GenerateCards();
    }

    private void GenerateCards()
    {
        stacks = Enumerable.Repeat(new List<Card>(), 10).ToArray();

        List<Global.Suits> suitPool = new List<Global.Suits>();
        switch(difficulty)
        {
            case Difficulty.Easy:
                suitPool.AddRange(Enumerable.Repeat(Global.Suits.Spades, 8));
                break;
            case Difficulty.Medium:
                suitPool.AddRange(Enumerable.Repeat(Global.Suits.Spades, 4));
                suitPool.AddRange(Enumerable.Repeat(Global.Suits.Hearts, 4));
                break;
            case Difficulty.Hard:
                suitPool.AddRange(Enumerable.Repeat(Global.Suits.Spades, 2));
                suitPool.AddRange(Enumerable.Repeat(Global.Suits.Hearts, 2));
                suitPool.AddRange(Enumerable.Repeat(Global.Suits.Diamonds, 2));
                suitPool.AddRange(Enumerable.Repeat(Global.Suits.Clubs, 2));
                break;
        }

        System.Random rand = new System.Random();
        int[] cardDistribution = Enumerable.Range(0, 104).OrderBy(x => rand.NextDouble()).ToArray();

        int cardIndex = 103;
        int row = 0;
        while(cardIndex >= 0)
        {
            foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
            {
                var stats = new CardStats();
                stats.denomination = cardDistribution[cardIndex] % 13;
                int suitAssignment = (int)Mathf.Floor(cardDistribution[cardIndex] / 13.0f);
                stats.suit = suitPool[suitAssignment];
                stats.turned = false;

                var newCard = Instantiate(cardPrefab);
                newCard.InitializeCard(stats);
                positioner.MoveCard(ref newCard, it.Index, row);
                it.Value.Add(newCard);

                cardIndex--;
                if(cardIndex == -1) break;
            }
            row++;
        }
    }
}
