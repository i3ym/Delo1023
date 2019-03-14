using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleItem
{
    public readonly Sprite image;
    public readonly Action action;
    public RectTransform transform;

    public CircleItem(Sprite image, Action action)
    {
        this.image = image;
        this.action = action;
    }
}