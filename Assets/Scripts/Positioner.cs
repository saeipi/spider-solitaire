using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Positioner : MonoSingleton<Positioner>
{
    [SerializeField] private Vector3 topRightPosition;
    [SerializeField] private Vector3 cardOffset;
    [SerializeField] private Vector3 deckPosition;
    [SerializeField] private Vector3 deckOffset;

    public void PositionStackHeader(StackHeader stackHeader, int stack)
    {
        stackHeader.transform.position = topRightPosition;
        stackHeader.transform.Translate(cardOffset.x * stack, 0, 1.0f);
    }

    public void MoveCard(ref Card card, int stack, int row)
    {
        card.transform.position = topRightPosition;
        card.transform.Translate(cardOffset.x * stack, -cardOffset.y * row, -0.1f * row);
    }

    public void PutIntoDeck(ref Card card, int deck)
    {
        card.transform.position = deckPosition;
        card.transform.Translate(deckOffset.x * deck, 0, -0.1f * deck);
    }
}
