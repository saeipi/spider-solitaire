using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Game))]
public class InputController : MonoSingleton<InputController>
{
    private Card draggedCard;
    private List<Card> draggedCardChildren;
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
                if (Input.GetMouseButtonDown(0) && Game.Instance.CheckMoveValidity(hitCard))
                {
                    draggedCard = hitCard;
                    draggedCardOriginalPosition = hitCard.transform.position;
                    hitCard.GetComponent<BoxCollider2D>().enabled = false;

                    draggedCardChildren = Game.Instance.GetCardChildren(draggedCard);
                    foreach(var children in draggedCardChildren)
                    {
                        children.transform.parent = draggedCard.transform;
                        children.GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
        }

        if (draggedCard && Input.GetMouseButtonUp(0))
        {
            bool moveAllowed = false;
            if(hitCard)
            {
                moveAllowed = Game.Instance.RequestCardMove(draggedCard, hitCard);
            }

            if(!hitCard || !moveAllowed)
            {
                draggedCard.transform.position = draggedCardOriginalPosition;
            }

            foreach (var children in draggedCardChildren)
            {
                children.transform.parent = null;
                children.GetComponent<BoxCollider2D>().enabled = true;
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
