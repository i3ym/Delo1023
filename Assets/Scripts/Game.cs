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
    public static Texture2D Atlas, AtlasMesh;
    public static bool Building = false;
    public static int Money { get => _money; set { _money = value; game.textMoney.text = "money: " + value.ToString(); } }
    public static int Villagers { get => _villagers; set { _villagers = value; game.textVillagers.text = "villagers: " + value.ToString(); } }
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
    public static Material material, materialMesh;
    public static World world;
    public static GameObject buildingChooser;
    public static List<Building> Buildings = new List<Building>();
    static Game game = null;
    static int _money = 0, _villagers = 0, _level = 0, _exp = 0;

    [SerializeField]
    Material mat, matMesh;
    [SerializeField]
    TextMeshProUGUI textMoney, textVillagers, textLevel, textExp;
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

        Texture2D[] textures = Resources.LoadAll<Texture2D>("Textures");
        Texture2D[] texturesMesh = Resources.LoadAll<Texture2D>("TexturesMesh");
        Mesh[] meshes = Resources.LoadAll<Mesh>("Models");

        Rect[] rects = Atlas.PackTextures(textures, 0);
        for (int i = 0; i < textures.Length; i++)
            TextureRects.Add(textures[i].name, rects[i]);

        AtlasMesh = new Texture2D(1, 1);
        AtlasMesh.filterMode = FilterMode.Point;
        AtlasMesh.wrapMode = TextureWrapMode.Repeat;

        rects = AtlasMesh.PackTextures(texturesMesh, 0);

        List<Vector2> uvlist = new List<Vector2>();
        for (int i = 0; i < meshes.Length; i++)
        {
            for (int j = 0; j < meshes[i].uv.Length; j++)
                uvlist.Add(new Vector2(meshes[i].uv[j].x * rects[i].width + rects[i].xMin, meshes[i].uv[j].y * rects[i].height + rects[i].yMin));

            TextureMeshUvs.Add(texturesMesh[i].name, uvlist.ToArray());
            uvlist.Clear();

            Meshes.Add(texturesMesh[i].name, meshes[i]);
            meshes[i].name = texturesMesh[i].name;
        }

        material = mat;
        material.mainTexture = Atlas;
        materialMesh = matMesh;
        materialMesh.mainTexture = AtlasMesh;

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