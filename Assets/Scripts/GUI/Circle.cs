using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Circle : MonoBehaviour
{
    const float selectedMultiplpier = 1.3f;
    const float multiplier = 1.7f;
    List<CircleItem> items = new List<CircleItem>();
    int count = 6;
    float size = 300f;
    new MeshRenderer renderer;
    Mesh mesh;

    [SerializeField]
    Sprite[] sprites = null;
    [SerializeField]
    Material materialSvg = null;

    void Start()
    {
        items.Add(new CircleItem(sprites[0], () =>
        {
            Debug.Log("copy");
        }));
        items.Add(new CircleItem(sprites[1], () =>
        {
            Debug.Log("paste");
        }));
        items.Add(new CircleItem(sprites[2], () =>
        {
            Debug.Log("regen");
        }));

        count = items.Count;

        GameObject go;
        SVGImage svg;
        float angle;
        for (int i = 0; i < items.Count; i++)
        {
            go = new GameObject("Image_" + i, typeof(RectTransform), typeof(SVGImage));
            svg = go.GetComponent<SVGImage>();
            svg.sprite = items[i].image;
            svg.material = materialSvg;
            svg.rectTransform.SetParent(transform);
            svg.rectTransform.localScale = Vector3.one;
            svg.rectTransform.sizeDelta = svg.sprite.bounds.extents * 300f;
            svg.transform.localPosition = -Vector3.one;

            angle = GetAngle(i);
            svg.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * size * .9f;

            items[i].transform = svg.rectTransform;
            Game.SvgImageToRaw(svg);
        }

        mesh = new Mesh();

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Unlit/Color"));
        gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) renderer.enabled = true;
        else if (Input.GetMouseButtonUp(1)) renderer.enabled = false;

        if (Input.GetMouseButton(1))
        {
            Vector2 mousePos = (Vector2) (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;

            float angle;
            int selected = mod(Mathf.FloorToInt(Mathf.Atan2(mousePos.y, mousePos.x) / Mathf.PI / 2f * count), count);

            CreateMesh(selected); //TODO not create but change

            for (int i = 0; i < items.Count; i++)
            {
                angle = GetAngle(i) + GetAngle(1) / 2f;
                if (i == selected) items[i].transform.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * size * .9f;
                else items[i].transform.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * size * .9f * selectedMultiplpier / multiplier;
            }

            if (Input.GetMouseButtonDown(0))
            {
                items[selected].action();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int mod(int x, int m) => (x % m + m) % m;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    float GetAngle(int i) => 2f * Mathf.PI / count * i;
    void CreateMesh(int selected = -1)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        float angle = GetAngle(0);
        Vector2 pos = new Vector2();

        for (int i = 0; i < count; i++)
        {
            pos.Set(Mathf.Cos(angle), Mathf.Sin(angle));
            if (i != selected)
            {
                verts.Add(pos * size);
                verts.Add(pos * multiplier * size);
            }
            else
            {
                verts.Add(pos * size * selectedMultiplpier);
                verts.Add(pos * multiplier * size * selectedMultiplpier);
            }

            angle = GetAngle(i + 1);
            pos.Set(Mathf.Cos(angle), Mathf.Sin(angle));
            if (i != selected)
            {
                verts.Add(pos * multiplier * size);
                verts.Add(pos * size);
            }
            else
            {
                verts.Add(pos * multiplier * size * selectedMultiplpier);
                verts.Add(pos * size * selectedMultiplpier);
            }

            tris.Add(2 + (i * 4));
            tris.Add(1 + (i * 4));
            tris.Add(i * 4);
            tris.Add(3 + (i * 4));
            tris.Add(2 + (i * 4));
            tris.Add(i * 4);
        }

        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.Optimize();
    }
}