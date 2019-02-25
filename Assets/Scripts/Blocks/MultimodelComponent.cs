using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultimodelComponent : BComponent
{
    public DeloMesh corner, side, center;

    public MultimodelComponent(string corner, string side, string center) // г т +
    {
        this.corner = new DeloMesh(Game.Meshes[corner]);
        this.side = new DeloMesh(Game.Meshes[side]);
        this.center = new DeloMesh(Game.Meshes[center]);
    }
}