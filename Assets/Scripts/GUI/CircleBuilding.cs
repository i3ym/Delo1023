using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBuilding : Circle
{
    void Awake()
    {
        items.Add(new CircleItem(sprites[0], () =>
        {
            Debug.Log("house");
        }));
        items.Add(new CircleItem(sprites[1], () =>
        {
            Debug.Log("building");
        }));
        items.Add(new CircleItem(sprites[2], () =>
        {
            Debug.Log("hospital");
        }));
        items.Add(new CircleItem(sprites[3], () =>
        {
            Debug.Log("police");
        }));
        items.Add(new CircleItem(sprites[4], () =>
        {
            Debug.Log("fire");
        }));
    }
}
