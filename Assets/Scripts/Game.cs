using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static new Camera camera;
    public static Dictionary<string, Rect> TextureRects = new Dictionary<string, Rect>();
    public static Dictionary<string, Vector2[]> TextureMeshUvs = new Dictionary<string, Vector2[]>();
    public static Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
    public static Dictionary<string, Texture2D> BlockRenders = new Dictionary<string, Texture2D>(); //TODO всё это запихнуть в один класс ?
    public static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    public static Texture2D Atlas;
    public static Material material;
    public static bool Building = false;
    public static World world;
    public static GameObject buildingChooser;
    public static List<Building> Buildings = new List<Building>();
    public static Game game = null;

    public static int Money { get => _money; set { _money = value; game.textMoney.text = value.ToString(); } }
    public static int Villagers { get => _villagers; set { _villagers = value; game.textVillagers.text = value + " / " + VillagersMax; } }
    public static int VillagersMax { get => _villagersMax; set { _villagersMax = value; Villagers = Math.Min(VillagersMax, Villagers); } }
    public static int Exp
    {
        get => _exp;
        set
        {
            _exp = value;
            int newlevel = LvlForExp(value);
            if (Level != newlevel) Level = newlevel;

            game.rainbowExperience.anchoredPosition = new Vector2(0f, 1f / ((ExpForLvl(Level + 1) - ExpForLvl(Level)) / (float) (value - ExpForLvl(Level))) * 116f);
        }
    }
    public static int Level
    {
        get => _level;
        private set
        {
            _level = value;
            game.textLevel.text = value.ToString();
        }
    }
    static int _money = 0, _villagers = 0, _level = 0, _exp = 0, _villagersMax;

    [SerializeField]
    Material mat = null;
    [SerializeField]
    TextMeshProUGUI textMoney = null, textVillagers = null, textLevel = null;
    [SerializeField]
    RectTransform rainbowExperience = null;
    [SerializeField]
    Mesh cubeMesh = null, cubeMeshMultitexture = null;

    void Awake()
    {
        if (game != null)
        {
            Destroy(this);
            return;
        }
        game = this;
        camera = Camera.main;
        BlockInfo.cubeMesh = cubeMesh;
        BlockInfo.cubeMeshMultitexture = cubeMeshMultitexture;

        CreateAtlas();

        foreach (Texture2D tex in Resources.LoadAll<Texture2D>("Textures")) textures.Add(tex.name, tex);
        foreach (Texture2D tex in Resources.LoadAll<Texture2D>("TexturesMesh")) textures.Add(tex.name, tex);

        Meshes.Add("transparent", new Mesh());
        TextureMeshUvs.Add("transparent", new Vector2[] { });

        Block.CreateBlocks();

        Money = 100000;
        Villagers = 0;
    }

    void CreateAtlas()
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

        material = mat;
        material.mainTexture = Atlas;
    }
    void CreateBlockRenders()
    {
        const int size = 256;

        GameObject camgo = GameObject.Find("RenderBlocksCamera");
        Camera camera = camgo.GetComponent<Camera>();
        RenderTexture rt = RenderTexture.GetTemporary(size, size);
        RenderTexture.active = rt;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0, 0, 0, 0);
        camera.targetTexture = rt;

        GameObject meshgo = new GameObject();
        meshgo.transform.eulerAngles = new Vector3(0f, 45f, 0f);
        meshgo.layer = 15;

        MeshFilter mf = meshgo.AddComponent<MeshFilter>();
        MeshRenderer mr = meshgo.AddComponent<MeshRenderer>();
        mr.material = new Material(mat);

        foreach (BlockInfo b in Block.Blocks)
        {
            Texture2D texture = new Texture2D(size, size);

            meshgo.transform.position = camgo.transform.position + new Vector3(0f, -1f, 1.5f);
            camera.transform.LookAt(meshgo.transform);

            if (Meshes.ContainsKey(b.Name))
            {
                mf.mesh = Meshes[b.Name];

                Vector3 sizef = mf.mesh.bounds.max - mf.mesh.bounds.min;
                Vector3Int meshsize = new Vector3Int(Mathf.CeilToInt(sizef.x), Mathf.CeilToInt(sizef.y), Mathf.CeilToInt(sizef.z));
                int meshmax = Math.Max(meshsize.x, meshsize.z);

                meshgo.transform.position += new Vector3(-(meshsize.z - 1) * .25f - (meshsize.x - 1) * .5f, -(meshsize.y - 1) - .5f, meshmax / 4f + meshsize.y / 1.5f - 1f);
            }
            else mf.mesh = cubeMesh;

            mr.material.mainTexture = textures[b.Name];

            camera.Render();
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture.Apply();

            BlockRenders.Add(b.Name, texture);

#if UNITY_EDITOR
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes("Render/" + b.Name + ".png", bytes);
#endif
        }

        Destroy(camgo);
        Destroy(meshgo);
        Destroy(rt);
    }

    static int ExpForLvl(int lvl) => (int) (Math.Sqrt(lvl) * lvl * 1000.0);
    static int LvlForExp(int exp)
    {
        for (int i = 0; true; i++)
            if (ExpForLvl(i) > exp) return i - 1;
    }
}