using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackHeader : MonoBehaviour
{
    private int stack;
    public int Stack
    {
        get
        {
            return stack;
        }
    }

    public void Initialize(int stack)
    {
        this.stack = stack;
    }
}
