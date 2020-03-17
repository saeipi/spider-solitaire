using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image[] suits;
    [SerializeField] private Text[] numbers;
    [SerializeField] private Sprite hearts, clubs, diamonds, spades;
    [SerializeField] private Sprite redBackground, blackBackground;
    private CardStats stats;

    public void InitializeCard(CardStats stats)
    {
        this.stats = stats;

        switch(stats.suit)
        {
            case Global.Suits.Hearts:
            case Global.Suits.Diamonds:
                foreach(var suit in suits)
                    suit.sprite = stats.suit == Global.Suits.Hearts ? hearts : diamonds;
                foreach (var number in numbers)
                    number.color = Color.red;
                break;
            case Global.Suits.Clubs:
            case Global.Suits.Spades:
                foreach (var suit in suits)
                    suit.sprite = stats.suit == Global.Suits.Clubs ? clubs : spades;
                foreach (var number in numbers)
                    number.color = Color.black;
                break;
        }

        foreach(var number in numbers)
            number.text = Global.denominations[stats.denomination];
    }

    public void TurnCard()
    {
        switch (stats.suit)
        {
            case Global.Suits.Hearts:
            case Global.Suits.Diamonds:
                background.sprite = redBackground;
                break;
            case Global.Suits.Clubs:
            case Global.Suits.Spades:
                background.sprite = blackBackground;
                break;
        }

        foreach (var number in numbers)
            number.gameObject.SetActive(true);

        foreach (var suit in suits)
            suit.gameObject.SetActive(true);

        stats.turned = true;
    }
}
