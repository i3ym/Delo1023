using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleActions : Circle
{
    void Awake()
    {
        items.Add(new CircleItem(sprites[0], () =>
        {
            Debug.Log("copy");
        }));
        items.Add(new CircleItem(sprites[1], () =>
        {
            Debug.Log("paste");
        }));
        items.Add(new CircleItem(sprites[2], () =>
        {
            Debug.Log("regen");
        }));
        
    }
}
