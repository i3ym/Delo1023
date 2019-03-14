using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Circle : MonoBehaviour
{
    protected const float selectedMultiplier = 1.1f;
    protected const int count = 9;
    protected List<CircleItem> items = new List<CircleItem>();
    protected float size = 300f;
    protected new MeshRenderer renderer;
    protected Mesh mesh;

    [SerializeField]
    protected Sprite[] sprites = null;
    [SerializeField]
    protected Material materialSvg = null;
    [SerializeField]
    protected float thickness, distance;
    [SerializeField]
    protected float inBound, outBound;

    void Start()
    {
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
            svg.rectTransform.sizeDelta = svg.sprite.bounds.extents * size;
            svg.transform.localPosition = -Vector3.one;

            angle = GetAngle(i) + GetAngle(1) / 2f;
            svg.rectTransform.anchoredPosition = GetPosition(angle) * selectedMultiplier;

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

            CreateMesh(selected);

            for (int i = 0; i < items.Count; i++)
            {
                if (i < items.Count) continue;

                angle = GetAngle(i) + GetAngle(1) / 2f;
                if (i == selected) items[i].transform.anchoredPosition = GetPosition(angle) * selectedMultiplier; //TODO
                else items[i].transform.anchoredPosition = GetPosition(angle) * selectedMultiplier; //TODO
            }

            if (Input.GetMouseButtonDown(0) && selected < items.Count) items[selected].action();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Vector2 GetPosition(float angle) => new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int mod(int x, int m) => (x % m + m) % m;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float GetAngle(int i) => 2f * Mathf.PI / count * i;
    void CreateMesh(int selected = -1)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        float angle = GetAngle(0);
        Vector2 pos = new Vector2();

        for (int i = 0; i < count; i++)
        {
            pos = GetPosition(angle);
            if (i == selected && i < items.Count)
            {
                verts.Add(pos.normalized * distance * selectedMultiplier);
                verts.Add(pos + pos.normalized * thickness * selectedMultiplier);
            }
            else
            {
                verts.Add(pos);
                verts.Add(pos + pos.normalized * thickness);
            }

            angle = GetAngle(i + 1);
            pos = GetPosition(angle);
            if (i == selected && i < items.Count)
            {
                verts.Add(pos + pos.normalized * thickness * selectedMultiplier);
                verts.Add(pos.normalized * distance * selectedMultiplier);
            }
            else
            {
                verts.Add(pos + pos.normalized * thickness);
                verts.Add(pos);
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