using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Difficulty
{
    Easy, Medium, Hard
}

[RequireComponent(typeof(Positioner))]
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
        stacks = new List<Card>[10];
        for (int i = 0; i < 10; i++) stacks[i] = new List<Card>();

        decks = new List<Card>[5];
        for (int i = 0; i < 5; i++) decks[i] = new List<Card>();

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
                for (int i = 0; i < 10; i++)
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

    private Card GenerateCard(int cardIndex, int[] cardDistribution, List<Global.Suits> suitPool)
    {
        var stats = new CardStats();
        stats.denomination = cardDistribution[cardIndex] % 13;
        int suitAssignment = (int)Mathf.Floor(cardDistribution[cardIndex] / 13.0f);
        stats.suit = suitPool[suitAssignment];
        stats.turned = false;
        stats.id = cardIndex;

        var newCard = Instantiate(cardPrefab);
        newCard.InitializeCard(stats);
        return newCard;
    }

    public bool RequestCardMove(Card movedCard, Card hoveredCard)
    {
        if (CheckMoveValidity(movedCard, hoveredCard))
        {
            MoveCard(movedCard, hoveredCard);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void MoveCard(Card movedCard, Card hoveredCard)
    {
        Stack<Card> movedCardChildren = new Stack<Card>();

        foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
        {
            if (it.Value.Exists(card => card.Equals(movedCard)))
            {
                var cardIndex = it.Value.IndexOf(movedCard);
                for(int i = it.Value.Count - 1; i > cardIndex; i--)
                {
                    movedCardChildren.Push(it.Value[i]);
                    it.Value.RemoveAt(i);
                }
                it.Value.Remove(movedCard);

                if(it.Value.Count > 0 && !it.Value.Last().Stats.turned)
                {
                    it.Value.Last().TurnCard();
                }
            }
        }

        foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
        {
            if (it.Value.Exists(card => card.Equals(hoveredCard)))
            {
                /* move card */
                positioner.MoveCard(ref movedCard, it.Index, it.Value.Count);
                it.Value.Add(movedCard);

                /* and all of its children */
                while(movedCardChildren.Count > 0)
                {
                    var movedChildren = movedCardChildren.Pop();
                    positioner.MoveCard(ref movedChildren, it.Index, it.Value.Count);
                    it.Value.Add(movedChildren);
                }
            }
        }
    }

    public bool CheckMoveValidity(Card movedCard)
    {
        /* if card isn't turned, we cannot move it */
        if (!movedCard.Stats.turned) return false;

        foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
        {
            if (it.Value.Exists(card => card.Equals(movedCard)))
            {
                var cardIndex = it.Value.IndexOf(movedCard);

                /* if card is at the bottom of stack, we're in the clear */
                if (cardIndex == it.Value.Count - 1) return true;

                /* otherwise, we need to check the children */
                for(int i = cardIndex + 1; i < it.Value.Count; i++)
                {
                    if (!IsStackMoveable(it.Value[i - 1], it.Value[i])) return false;
                }
                return true;
            }
        }
        return false;
    }

    public bool CheckMoveValidity(Card movedCard, Card hoveredCard)
    {
        foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
        {
            if (it.Value.Exists(card => card.Equals(hoveredCard)))
            {
                var topCard = it.Value.Last();
                return IsCardStackable(movedCard, topCard);
            }
        }
        return false;
    }

    public bool IsCardStackable(Card topCard, Card bottomCard)
    {
        return topCard.Stats.denomination == bottomCard.Stats.denomination - 1;
    }

    public bool IsStackMoveable(Card topCard, Card bottomCard)
    {
        /* we have to fullfill stack criteria and have matching suits */
        return IsCardStackable(bottomCard, topCard)
            && topCard.Stats.suit == bottomCard.Stats.suit;
    }
}
