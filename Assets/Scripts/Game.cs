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
    private List<Card>[] decks;

    void Start()
    {
        GenerateCards();
    }

    private void GenerateCards()
    {
        stacks = Enumerable.Repeat(new List<Card>(), 10).ToArray();
        decks = Enumerable.Repeat(new List<Card>(), 5).ToArray();

        List<Global.Suits> suitPool = new List<Global.Suits>();
        switch (difficulty)
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

        while (cardIndex > 53)
        {
            foreach (var it in decks.Select((x, y) => new { Value = x, Index = y }))
            {
                for(int i = 0; i < 10; i++)
                {
                    Card newCard = GenerateCard(cardIndex, cardDistribution, suitPool);
                    positioner.PutIntoDeck(ref newCard, it.Index);
                    it.Value.Add(newCard);
                    cardIndex--;
                }
            }
            
        }
            
        int row = 0;
        while (cardIndex > 0)
        {
            foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
            {
                Card newCard = GenerateCard(cardIndex, cardDistribution, suitPool);
                positioner.MoveCard(ref newCard, it.Index, row);
                it.Value.Add(newCard);

                if (cardIndex < 10) newCard.TurnCard();
                if (cardIndex == 0) break;
                cardIndex--;
            }
            row++;
        }
    }

    private Card GenerateCard(int cardIndex, int[] cardDistribution, List<Global.Suits> suitPool) {
        var stats = new CardStats();
        stats.denomination = cardDistribution[cardIndex] % 13;
        int suitAssignment = (int)Mathf.Floor(cardDistribution[cardIndex] / 13.0f);
        stats.suit = suitPool[suitAssignment];
        stats.turned = false;

        var newCard = Instantiate(cardPrefab);
        newCard.InitializeCard(stats);
        return newCard;
    }
}
