using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultimodelComponent : BComponent
{
    public Mesh corner, side, center;

    public MultimodelComponent(string corner, string side, string center) // г т +
    {
        this.corner = Game.Meshes[corner];
        this.side = Game.Meshes[side];
        this.center = Game.Meshes[center];
    }
}