using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public static new Camera camera;
    public static Dictionary<string, ModelHolder> models = new Dictionary<string, ModelHolder>();
    public static Dictionary<string, Rect> TextureRects = new Dictionary<string, Rect>();
    public static Dictionary<string, Vector2[]> TextureMeshUvs = new Dictionary<string, Vector2[]>();
    public static Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
    public static Texture2D Atlas;
    public static Material material;
    public static bool Building = false;
    public static int Money { get => _money; set { _money = value; game.textMoney.text = "money: " + value.ToString(); } }
    public static int Villagers { get => _villagers; set { _villagers = value; game.textVillagers.text = "villagers: " + value.ToString(); } }
    public static int VillagersMax { get => _villagersMax; set { _villagersMax = value; Villagers = Math.Min(VillagersMax, Villagers); game.textVillagersMax.text = "villagersMax: " + value.ToString(); } }
    public static int Exp
    {
        get => _exp;
        set
        {
            _exp = value;
            Level = LvlForExp(value);
            game.sliderExp.value = value - ExpForLvl(Level);
            game.textExp.text = "exp: " + (value - ExpForLvl(Level));
        }
    }
    public static int Level
    {
        get => _level;
        private set
        {
            _level = value;
            game.sliderExp.maxValue = (float) (ExpForLvl(value + 1) - ExpForLvl(value));
            game.textLevel.text = "level: " + value.ToString();
        }
    }
    public static World world;
    public static GameObject buildingChooser;
    public static List<Building> Buildings = new List<Building>();
    static Game game = null;
    static int _money = 0, _villagers = 0, _level = 0, _exp = 0, _villagersMax;

    [SerializeField]
    Material mat;
    [SerializeField]
    TextMeshProUGUI textMoney, textVillagers, textLevel, textExp, textVillagersMax;
    [SerializeField]
    Slider sliderExp;

    void Awake()
    {
        if (game != null)
        {
            Destroy(this);
            return;
        }
        game = this;
        camera = Camera.main;

        Atlas = new Texture2D(1, 1);
        Atlas.filterMode = FilterMode.Point;
        Atlas.wrapMode = TextureWrapMode.Repeat;

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

        Meshes.Add("transparent", new Mesh());
        TextureMeshUvs.Add("transparent", new Vector2[] { });

        Money = 100000;
        Villagers = 0;
    }

    static int ExpForLvl(int lvl) => (int) (Math.Sqrt(lvl) * lvl * 1000.0);
    static int LvlForExp(int exp)
    {
        for (int i = 0; true; i++)
            if (ExpForLvl(i) > exp) return i - 1;
    }
}