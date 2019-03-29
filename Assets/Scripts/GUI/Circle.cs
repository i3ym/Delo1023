using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Circle : MonoBehaviour
{
    protected const float selectedMultiplier = 1.1f;
    public static bool isActive = false;
    protected static int count = 0;
    protected List<CircleItem> items = new List<CircleItem>();
    protected new MeshRenderer renderer;
    protected Mesh mesh;
    protected new RectTransform transform;
    protected Vector3 mouseStartPos;

    [SerializeField]
    protected Sprite[] sprites = null;
    [SerializeField]
    protected Material materialSvg = null, materialCircle = null;
    [SerializeField]
    protected float thickness, distance;
    [SerializeField]
    protected float inBound, outBound;
    [SerializeField]
    Canvas canvas = null;

    void Awake()
    {
        AddItems();
        if (count < items.Count) count = items.Count;
    }
    void Start()
    {
        transform = GetComponent<RectTransform>();

        if (outBound < 0) outBound = float.MaxValue;

        GameObject go;
        SVGImage svg;
        for (int i = 0; i < items.Count; i++)
        {
            go = new GameObject("Image_" + i, typeof(RectTransform), typeof(SVGImage));
            svg = go.GetComponent<SVGImage>();
            svg.sprite = items[i].image;
            svg.material = materialSvg;
            svg.rectTransform.SetParent(transform);
            svg.rectTransform.localScale = Vector3.one;
            svg.rectTransform.sizeDelta = svg.sprite.bounds.extents * thickness * 2f;
            svg.transform.localPosition = -Vector3.one;

            svg.rectTransform.anchoredPosition = GetIconPosition(i);

            items[i].transform = svg.rectTransform;
            Game.SvgImageToRaw(svg);

            go.SetActive(false);
        }

        mesh = new Mesh();

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = materialCircle;
        gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !Game.Building)
        {
            renderer.enabled = isActive = true;
            foreach (CircleItem item in items) item.transform.gameObject.SetActive(true);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out Vector2 pos);
            transform.anchoredPosition = pos;
            mouseStartPos = pos;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            renderer.enabled = isActive = false;
            foreach (CircleItem item in items) item.transform.gameObject.SetActive(false);
        }

        if (Input.GetMouseButton(1))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out Vector2 mousePos);
            mousePos -= (Vector2) mouseStartPos;
            if (mousePos.magnitude < inBound || mousePos.magnitude > outBound)
            {
                CreateMesh();
                for (int i = 0; i < items.Count; i++)
                    items[i].transform.anchoredPosition = GetIconPosition(i);

                return;
            }

            int selected = Mod(Mathf.FloorToInt(Mathf.Atan2(mousePos.y, mousePos.x) / Mathf.PI / 2f * count), count);

            CreateMesh(selected);

            for (int i = 0; i < items.Count; i++)
            {
                if (i == selected) items[i].transform.anchoredPosition = GetIconPosition(i) * selectedMultiplier;
                else items[i].transform.anchoredPosition = GetIconPosition(i);
            }

            if (Input.GetMouseButtonDown(0) && selected < items.Count) items[selected].action();
        }
    }

    protected abstract void AddItems();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Vector2 GetPosition(float angle) => new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int Mod(int x, int m) => (x % m + m) % m;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static float GetAngle(int i) => 2f * Mathf.PI / count * i; // Mod(i, count);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Vector2 GetIconPosition(int i) => (GetPosition(GetAngle(i)) + GetPosition(GetAngle(i + 1))).normalized * (distance + thickness / 4f);
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