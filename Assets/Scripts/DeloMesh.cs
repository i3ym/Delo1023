using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeloMesh
{
    public Mesh mesh;
    public string name;
    public Vector3[] vertices { get => _vertices; set => mesh.vertices = _vertices = value; }
    public int[] triangles { get => _triangles; set => mesh.triangles = _triangles = value; }
    public Vector2[] uv { get => _uv; set => mesh.uv = _uv = value; }

    Vector3[] _vertices;
    int[] _triangles;
    Vector2[] _uv;

    public DeloMesh(Mesh mesh)
    {
        this.mesh = mesh;
        name = mesh.name;
        _vertices = mesh.vertices;
        _triangles = mesh.triangles;
        _uv = mesh.uv;
    }
}