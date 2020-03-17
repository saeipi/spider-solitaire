using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Positioner : MonoBehaviour
{
    [SerializeField] private Vector3 topRightPosition;
    [SerializeField] private Vector3 cardOffset;

    public void MoveCard(ref Card card, int stack, int row)
    {
        card.transform.position = topRightPosition;
        card.transform.Translate(cardOffset.x * stack, -cardOffset.y * row, 0);
    }
}
