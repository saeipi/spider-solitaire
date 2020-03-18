using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Difficulty
{
    Easy, Medium, Hard
}

[RequireComponent(typeof(Positioner))]
public class Game : MonoSingleton<Game>
{
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private StackHeader stackHeaderPrefab;
    private List<Card>[] stacks;
    private List<List<Card>> decks;

    void Start()
    {
        SpawnStackHeaders();
        GenerateCards();
    }

    public void ResetGame(Difficulty difficulty)
    {
        this.difficulty = difficulty;

        /* remove all cards */
        foreach(var stack in stacks)
        {
            foreach(var card in stack)
            {
                Destroy(card.gameObject);
            }
        }

        foreach (var deck in decks)
        {
            foreach (var card in deck)
            {
                Destroy(card.gameObject);
            }
        }

        GenerateCards();
    }

    private void SpawnStackHeaders()
    {
        for(int i = 0; i < 10; i++)
        {
            StackHeader header = Instantiate(stackHeaderPrefab);
            header.Initialize(i);
            Positioner.Instance.PositionStackHeader(header, i);
        }
    }

    private void GenerateCards()
    {
        stacks = new List<Card>[10];
        for (int i = 0; i < 10; i++) stacks[i] = new List<Card>();

        decks = new List<List<Card>>();
        for (int i = 0; i < 5; i++) decks.Add(new List<Card>());

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
                    Positioner.Instance.PutIntoDeck(ref newCard, it.Index);
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
                Positioner.Instance.MoveCard(ref newCard, it.Index, row);
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
        stats.denomination = cardDistribution[cardIndex] % Global.denominations.Length;
        int suitAssignment = (int)Mathf.Floor(cardDistribution[cardIndex] / (float)Global.denominations.Length);
        stats.suit = suitPool[suitAssignment];
        stats.turned = false;
        stats.id = cardIndex;

        var newCard = Instantiate(cardPrefab);
        newCard.Initialize(stats);
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

    public bool RequestCardMove(Card movedCard, StackHeader stackHeader)
    {
        if(CheckMoveValidity(movedCard, stackHeader))
        {
            MoveCard(movedCard, stackHeader);
            return true;
        }
        else
        {
            return false;
        }
    }

    private Stack<Card> CutCardChildren(Card movedCard)
    {
        Stack<Card> movedCardChildren = new Stack<Card>();

        foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
        {
            if (it.Value.Exists(card => card.Equals(movedCard)))
            {
                var cardIndex = it.Value.IndexOf(movedCard);
                for (int i = it.Value.Count - 1; i > cardIndex; i--)
                {
                    movedCardChildren.Push(it.Value[i]);
                    it.Value.RemoveAt(i);
                }
                it.Value.Remove(movedCard);

                if (it.Value.Count > 0 && !it.Value.Last().Stats.turned)
                {
                    it.Value.Last().TurnCard();
                }
            }
        }

        return movedCardChildren;
    }

    public void MoveCard(Card movedCard, Card hoveredCard)
    {
        var movedCardChildren = CutCardChildren(movedCard);

        foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
        {
            if (it.Value.Exists(card => card.Equals(hoveredCard)))
            {
                /* move card */
                Positioner.Instance.MoveCard(ref movedCard, it.Index, it.Value.Count);
                it.Value.Add(movedCard);

                /* and all of its children */
                while(movedCardChildren.Count > 0)
                {
                    var movedChildren = movedCardChildren.Pop();
                    Positioner.Instance.MoveCard(ref movedChildren, it.Index, it.Value.Count);
                    it.Value.Add(movedChildren);
                }

                /* check whether we have created a tableau */
                CheckTableau(it.Index);
            }
        }
    }

    public void MoveCard(Card movedCard, StackHeader stackHeader)
    {
        var movedCardChildren = CutCardChildren(movedCard);

        /* move card */
        Positioner.Instance.MoveCard(ref movedCard, stackHeader.Stack, 0);
        stacks[stackHeader.Stack].Add(movedCard);

        /* and all of its children */
        while (movedCardChildren.Count > 0)
        {
            var movedChildren = movedCardChildren.Pop();
            Positioner.Instance.MoveCard(ref movedChildren, stackHeader.Stack, stacks[stackHeader.Stack].Count);
            stacks[stackHeader.Stack].Add(movedChildren);
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

    public bool CheckMoveValidity(Card movedCard, StackHeader stackHeader)
    {
        /* card must be a king and the stack in question empty */
        return stacks[stackHeader.Stack].Count == 0
            && movedCard.Stats.denomination == Global.denominations.Length - 1;
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

    public List<Card> GetCardChildren(Card parentCard)
    {
        List<Card> children = new List<Card>();
        foreach (var it in stacks.Select((x, y) => new { Value = x, Index = y }))
        {
            if (it.Value.Exists(card => card.Equals(parentCard)))
            {
                var cardIndex = it.Value.IndexOf(parentCard);
                for (int i = it.Value.Count - 1; i > cardIndex; i--)
                {
                    children.Add(it.Value[i]);
                }
            }
        }
        return children;
    }

    public void DealFromDeck()
    {
        if(decks.Count > 0)
        {
            var dealedDeck = decks.Last();
            decks.RemoveAt(decks.Count - 1);

            for(int i = 0; i < 10; i++)
            {
                var dealedCard = dealedDeck[i];
                Positioner.Instance.MoveCard(ref dealedCard, i, stacks[i].Count);
                dealedCard.TurnCard();
                stacks[i].Add(dealedCard);
            }
        }
    }

    public bool CheckTableau(int stack)
    {
        int stackCount = stacks[stack].Count;

        /* tableau consists of a complete set of cards (A-K) */
        if (stackCount < Global.denominations.Length) return false;

        for(int i = stackCount - 1; i > stackCount - Global.denominations.Length; i--)
        {
            if (!IsStackMoveable(stacks[stack][i - 1], stacks[stack][i])) return false;
        }

        RemoveTableau(stack);
        return true;
    }

    public void RemoveTableau(int stack)
    {
        int stackCount = stacks[stack].Count;
        for (int i = stackCount - 1; i >= stackCount - Global.denominations.Length; i--)
        {
            var removedCard = stacks[stack][i];
            stacks[stack].RemoveAt(i);
            Destroy(removedCard.gameObject);
        }

        if(stacks[stack].Count > 0)
        {
            stacks[stack].Last().TurnCard();
        }
    }
}
