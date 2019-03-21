using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleBuilding : Circle
{
    override protected void AddItems()
    {
        items.Add(new CircleItem(sprites[0], () => Game.world.StartBuilding<BuildingHouse>()));
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
        items.Add(new CircleItem(sprites[5], () =>
        {
            Debug.Log("school");
        }));
        items.Add(new CircleItem(sprites[6], () =>
        {
            Debug.Log("shop");
        }));
        items.Add(new CircleItem(sprites[7], () =>
        {
            Debug.Log("energy");
        }));
        items.Add(new CircleItem(sprites[8], () =>
        {
            Debug.Log("trash");
        }));
    }
}