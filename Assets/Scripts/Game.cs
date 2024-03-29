﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static new Camera camera;
    public static Transform cameratr;
    public static Dictionary<string, Rect> TextureRects = new Dictionary<string, Rect>();
    public static Dictionary<string, Vector2[]> TextureMeshUvs = new Dictionary<string, Vector2[]>();
    public static Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
    public static Dictionary<string, Texture2D> BlockRenders = new Dictionary<string, Texture2D>(); //TODO всё это запихнуть в один класс ?
    public static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    public static Texture2D Atlas;
    public static Material material, materialSelected, materialUnselected;
    public static bool Building = false;
    public static bool Paused = false;
    public static World world;
    public static Game game = null;
    public static ChunkCopy[] CopiedChunks;
    public static List<Building> Buildings = new List<Building>();
    static List<Action> Actions = new List<Action>();
    static bool invoke = false;

    public static long Money { get => _money; set { _money = value; Invoke(() => game.textMoney.text = value.ToString()); } }
    public static long Villagers { get => _villagers; set { _villagers = value; Invoke(() => game.textVillagers.text = value + " / " + VillagersMax); } }
    public static long VillagersMax { get => _villagersMax; set { _villagersMax = value; Villagers = Math.Min(VillagersMax, Villagers); } }
    public static long Exp
    {
        get => _exp;
        set
        {
            _exp = value;
            int newlevel = LvlForExp(value);
            if (Level != newlevel) Level = newlevel;

            Invoke(() => game.rainbowExperience.anchoredPosition = new Vector2(0f, 1f / ((ExpForLvl(Level + 1) - ExpForLvl(Level)) / (float) (value - ExpForLvl(Level))) * 116f));
        }
    }
    public static int Level
    {
        get => _level;
        private set
        {
            _level = value;
            Invoke(() => game.textLevel.text = value.ToString());
        }
    }
    static long _money = 0, _villagers = 0, _exp = 0, _villagersMax = 0;
    static int _level = 0;

    [SerializeField]
    Material mat = null;
    [SerializeField]
    TextMeshProUGUI textMoney = null, textVillagers = null, textLevel = null;
    [SerializeField]
    RectTransform rainbowExperience = null;

    void Awake()
    {
        if (game != null)
        {
            Destroy(this);
            return;
        }
        game = this;
        camera = Camera.main;
        cameratr = camera.transform;
        material = mat;
        materialSelected = new Material(material);
        materialSelected.color = Color.green;
        materialUnselected = new Material(material);
        materialUnselected.color = Color.gray;
        Money = 100000;
        VillagersMax = 0;
        Exp = 0;

        CreateAtlas();

        material.mainTexture = Atlas;
        materialSelected.mainTexture = Atlas;
        materialUnselected.mainTexture = Atlas;

        foreach (Texture2D tex in Resources.LoadAll<Texture2D>("Textures")) textures.Add(tex.name, tex);
        foreach (Texture2D tex in Resources.LoadAll<Texture2D>("TexturesMesh")) textures.Add(tex.name, tex);

        Meshes.Add("transparent", new Mesh());
        TextureMeshUvs.Add("transparent", new Vector2[] { });

        Block.CreateBlocks();

        StartCoroutine(InvokerCoroutine());
    }

    IEnumerator InvokerCoroutine()
    {
        WaitUntil wait = new WaitUntil(() => invoke);

        while (true)
        {
            yield return wait;
            foreach (Action action in Actions) action();
            Actions.Clear();
            invoke = false;
        }
    }
    public static void Invoke(Action action)
    {
        if (Actions.Contains(action)) return;

        lock(Actions)
        {
            Actions.Add(action);
            invoke = true;
        }
    }
    public static void SvgImageToRaw(SVGImage img)
    {
        return;
        GameObject obj = img.gameObject;
        Texture2D texx = VectorUtils.RenderSpriteToTexture2D(img.sprite, (int) img.rectTransform.sizeDelta.x, (int) img.rectTransform.sizeDelta.y, img.material, 8, true);
        texx.wrapMode = TextureWrapMode.Clamp;

        DestroyImmediate(img);
        obj.AddComponent<RawImage>().texture = texx;
    }
    public static void SvgImagesToRaw()
    {
        foreach (SVGImage img in GameObject.FindObjectsOfType<SVGImage>()) SvgImageToRaw(img);
    }
    static void CreateAtlas()
    {
        Atlas = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        Atlas.filterMode = FilterMode.Point;
        Atlas.wrapMode = TextureWrapMode.Clamp;

        Texture2D[] texturesBlocks = Resources.LoadAll<Texture2D>("Textures");
        Texture2D[] texturesMeshes = Resources.LoadAll<Texture2D>("TexturesMesh");
        Texture2D[] textures = new Texture2D[texturesBlocks.Length + texturesMeshes.Length];
        texturesBlocks.CopyTo(textures, 0);
        texturesMeshes.CopyTo(textures, texturesBlocks.Length);

        Mesh[] meshes = Resources.LoadAll<Mesh>("Models");

        Rect[] rects = Atlas.PackTextures(textures, 0);
        for (int i = 0; i < texturesBlocks.Length; i++)
            TextureRects.Add(texturesBlocks[i].name, rects[i]);

        List<Vector2> uvlist = new List<Vector2>();
        for (int i = 0; i < texturesMeshes.Length; i++)
        {
            for (int j = 0; j < meshes[i].uv.Length; j++)
                uvlist.Add(new Vector2(meshes[i].uv[j].x * rects[i + texturesBlocks.Length].width + rects[i + texturesBlocks.Length].xMin, meshes[i].uv[j].y * rects[i + texturesBlocks.Length].height + rects[i + texturesBlocks.Length].yMin));

            TextureMeshUvs.Add(texturesMeshes[i].name, uvlist.ToArray());
            uvlist.Clear();

            Meshes.Add(texturesMeshes[i].name, meshes[i]);
            meshes[i].name = texturesMeshes[i].name;
        }
    }

    static int ExpForLvl(int lvl) => (int) (Math.Sqrt(lvl) * lvl * 1000.0);
    static int LvlForExp(long exp)
    {
        for (int i = 0; true; i++)
            if (ExpForLvl(i) > exp) return i - 1;
    }
}