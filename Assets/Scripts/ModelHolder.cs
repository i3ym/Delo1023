using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelHolder
{
    public Mesh mesh { get; private set; }
    public Vector2 uv { get; private set; }

    public ModelHolder(Mesh mesh, Vector2 uv)
    {
        this.mesh = mesh;
        this.uv = uv;
    }
}