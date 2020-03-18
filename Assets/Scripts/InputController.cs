using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private Game game;
    private Card draggedCard;
    private Vector3 draggedCardOriginalPosition;

    void Update()
    {
        Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);
        Card hitCard = null;

        if (hit)
        {
            hitCard = hit.transform.gameObject.GetComponent<Card>();

            if(hitCard)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    draggedCard = hitCard;
                    draggedCardOriginalPosition = hitCard.transform.position;
                    hitCard.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            bool moveAllowed = false;
            if(hitCard)
            {
                moveAllowed = game.RequestCardMove(draggedCard, hitCard);
            }

            if(!hitCard || !moveAllowed)
            {
                draggedCard.transform.position = draggedCardOriginalPosition;
            }

            draggedCard.GetComponent<BoxCollider2D>().enabled = true;
            draggedCard = null;
        }

        if(draggedCard)
        {
            draggedCard.gameObject.transform.position = new Vector3(ray.x, ray.y, -5.0f);
        }
    }
}
