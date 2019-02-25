using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeloMesh
{
    public Mesh mesh;
    public string name;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uv;

    public DeloMesh(Mesh mesh)
    {
        this.mesh = mesh;
        name = mesh.name;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        uv = mesh.uv;
    }
}