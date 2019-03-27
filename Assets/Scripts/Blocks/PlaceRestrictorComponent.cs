using System;
using UnityEngine;

public class PlaceRestrictorComponent : BComponent
{
    Func<int, int, int, int, bool> action;

    public PlaceRestrictorComponent(Func<int, int, int, int, bool> action)
    {
        this.action = action;
    }

    public override bool OnPlace(int x, int y, int z, int rot) => action(x, y, z, rot);
}