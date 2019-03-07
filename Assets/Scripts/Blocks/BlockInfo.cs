using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BlockInfo
{
    public string Name { get; protected set; }
    public int Price { get; protected set; }
    public bool IsTransparent { get; protected set; }
    public Vector2[] uvs { get; protected set; } // 0-3 right  4-7 left  8-11 top  12-15 bottom  16-19 front  20-23 back
    public DeloMesh mesh { get; protected set; } = null;
    protected Dictionary<Type, object[]> Components = new Dictionary<Type, object[]>();
    protected Block _instance = null;

    protected const int renderSize = 256;
    public static GameObject camgo, meshgo;
    protected static MeshFilter mf;
    protected static MeshRenderer mr;
    protected static Camera renderCamera;
    public static Mesh cubeMesh, cubeMeshMultitexture;

    static Rect tempUv;
    static Vector2[] tempUvs;
    static List<Vector2> templistUv = new List<Vector2>();

    public BlockInfo() { }
    public BlockInfo(string name, string[] textures = null, bool isTransparent = false, int price = 1, Dictionary<Type, object[]> components = null)
    {
        templistUv.Clear();

        if (textures != null)
        {
            if (textures.Length != 6) throw new System.ArgumentException("Size of textures array is not 6");

            templistUv.AddRange(To2(textures[0]));
            templistUv.AddRange(To2(textures[1]));
            templistUv.AddRange(To2(textures[2]));
            templistUv.AddRange(To2(textures[3]));
            templistUv.AddRange(To2(textures[4]));
            templistUv.AddRange(To2(textures[5]));
        }
        else
        {
            tempUvs = To2(name);

            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);
        }

        this.Name = name;
        this.uvs = templistUv.ToArray();
        this.IsTransparent = isTransparent;
        this.mesh = mesh;
        Price = price;

        Block.Blocks.Add(this);
        if (components != null) Components = components;

        Game.BlockRenders.Add(Name, Render(textures));
    }

    public static void CreateGameObjectsToRender()
    {
        camgo = GameObject.Find("RenderBlocksCamera");
        renderCamera = camgo.GetComponent<Camera>();
        RenderTexture rt = RenderTexture.GetTemporary(renderSize, renderSize);
        RenderTexture.active = rt;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        renderCamera.targetTexture = rt;

        meshgo = new GameObject();
        meshgo.transform.eulerAngles = new Vector3(0f, 45f, 0f);
        meshgo.layer = 15;

        mf = meshgo.AddComponent<MeshFilter>();
        mr = meshgo.AddComponent<MeshRenderer>();
        mr.material = new Material(Game.material);
    }

    protected Texture2D Render(string[] textures = null)
    {
        Texture2D outtexture = new Texture2D(renderSize, renderSize);

        meshgo.transform.position = camgo.transform.position + new Vector3(0f, -1f, 1.5f);
        renderCamera.transform.LookAt(meshgo.transform);

        if (this is BlockInfoMesh)
        {
            mf.mesh = Game.Meshes[Name];

            Vector3 sizef = mf.mesh.bounds.max - mf.mesh.bounds.min;
            Vector3Int meshsize = new Vector3Int(Mathf.CeilToInt(sizef.x), Mathf.CeilToInt(sizef.y), Mathf.CeilToInt(sizef.z));
            int meshmax = Math.Max(meshsize.x, meshsize.z);

            meshgo.transform.position += new Vector3(-(meshsize.z - 1) * .25f - (meshsize.x - 1) * .5f, -(meshsize.y - 1) - .5f, meshmax / 4f + meshsize.y / 1.5f - 1f);
        }
        else mf.mesh = cubeMesh;

        if (textures == null) mr.material.mainTexture = Game.textures[Name];
        else
        {
            mf.mesh = cubeMeshMultitexture;

            Texture2D tex = new Texture2D(Game.textures[textures[0]].width * 4, Game.textures[textures[0]].height * 4);

            for (int i = 0; i < 6; i++)
                tex.SetPixels((i % 4) * tex.width / 4, (i / 4) * tex.height / 4, tex.width / 4, tex.height / 4, Game.textures[textures[i]].GetPixels());
            tex.Apply();

            mr.material.mainTexture = tex;
        }

        renderCamera.Render();
        outtexture.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
        outtexture.Apply();
        outtexture.SetPixel(0, 0, Color.black);

        return outtexture;
    }
    static Vector2[] To2(string tex)
    {
        tempUv = Game.TextureRects[tex];

        return new Vector2[] { new Vector2(tempUv.xMin, tempUv.yMin), new Vector2(tempUv.xMin, tempUv.yMax), new Vector2(tempUv.xMax, tempUv.yMax), new Vector2(tempUv.xMax, tempUv.yMin) };
    }

    public Block Instance()
    {
        if (_instance != null) return _instance;

        Block b = new Block();
        b.Info = this;

        foreach (var c in Components)
            b.AddComponent(c.Key, c.Value);

        return b;
    }
}

public class BlockInfoMesh : BlockInfo
{
    public BlockInfoMesh() { }
    public BlockInfoMesh(string name, int price = 1, Dictionary<Type, object[]> components = null)
    {
        this.Name = name;
        this.uvs = Game.TextureMeshUvs[name];
        this.IsTransparent = true;
        this.mesh = new DeloMesh(Game.Meshes[name]);
        Price = price;

        Vector3 sizef = mesh.mesh.bounds.max - mesh.mesh.bounds.min;
        Vector3Int size = new Vector3Int(Mathf.CeilToInt(sizef.x), Mathf.CeilToInt(sizef.y), Mathf.CeilToInt(sizef.z));

        if (size.x > 1 || size.y > 1 || size.z > 1)
        {
            List<Vector3Int> args = new List<Vector3Int>();
            if (components == null) components = new Dictionary<Type, object[]>();

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    for (int z = 0; z < size.z; z++)
                        args.Add(new Vector3Int(x, y, z));

            components.Add(typeof(MultiblockComponent), new object[] { args.ToArray(), this });
        }

        Block.Blocks.Add(this);
        if (components != null) Components = components;

        if (name != "transparent") Game.BlockRenders.Add(Name, Render());
    }
}