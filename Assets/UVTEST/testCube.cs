using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class testCube : MonoBehaviour
{
    [SerializeField]
    Mesh cube;
    [SerializeField]
    Material material;

    void Start()
    {
        Vector2[] uvs = new Vector2[cube.vertices.Length];

        Dictionary < (int, int, int), int > sides = new Dictionary < (int, int, int), int > ()
        {
            {
            (-1, 0, 0), 0
            },
            {
            (1, 0, 0),
            1
            },
            {
            (0, 1, 0),
            2
            },
            {
            (0, -1, 0),
            3
            },
            {
            (0, 0, 1),
            4
            },
            {
            (0, 0, -1),
            5
            }
        };

        byte[] cubesides = new byte[] { 0, 0, 1, 0, 0, 0 };

        byte max = cubesides.Max();
        float multx, multy;

        if (max == 0) multx = multy = 1f;
        else if (max == 1)
        {
            multx = .5f;
            multy = 1f;
        }
        else if (max > 1 && max < 4) multx = multy = .5f;
        else
        {
            multx = .25f;
            multy = .5f;
        }

        int side;
        int x, y;

        for (int i = 0; i < uvs.Length; i++)
        {
            side = sides[(Mathf.RoundToInt(cube.normals[i].x), Mathf.RoundToInt(cube.normals[i].y), Mathf.RoundToInt(cube.normals[i].z))];

            x = cubesides[side];
            y = cubesides[side];

            if (side == 0 || side == 1) uvs[i] = new Vector2(x * multx + multx * (cube.vertices[i].z + .5f), y * multy + multy * (1f-cube.vertices[i].y + .5f));
            else if (side == 2 || side == 3) uvs[i] = new Vector2(x * multx + multx * (cube.vertices[i].x + .5f), y * multy + multy * (1f-cube.vertices[i].z + .5f));
            else if (side == 4 || side == 5) uvs[i] = new Vector2(x * multx + multx * (cube.vertices[i].x + .5f), y * multy + multy * (1f-cube.vertices[i].y + .5f));
        }

        cube.uv = uvs;

        GameObject go = new GameObject("testCUBE");
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        go.AddComponent<MeshFilter>().mesh = cube;
        go.transform.position = new Vector3(-5, -5, -5);
    }
}