using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockInfo
{
    public string Name { get; protected set; }
    public int Price { get; protected set; }
    public bool IsTransparent { get; protected set; }
    public Vector2[] uvs { get; protected set; } // 0-3 left  4-7 right  8-11 top  12-15 bottom  16-19 front  20-23 back
    public Mesh mesh { get; protected set; } = null;
    protected Dictionary<Type, object[]> Components = new Dictionary<Type, object[]>();
    protected Block _instance = null;

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
            templistUv.AddRange(To2Rotated(textures[4]));
            templistUv.AddRange(To2Rotated(textures[5]));
        }
        else
        {
            tempUvs = To2(name);

            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);

            tempUvs = To2Rotated(name);
            templistUv.AddRange(tempUvs);
            templistUv.AddRange(tempUvs);
        }

        this.Name = name;
        this.uvs = templistUv.ToArray();
        this.IsTransparent = isTransparent;
        this.mesh = mesh;
        Price = price;

        Builder.Blocks.Add(this);
        if (components != null) Components = components;
    }

    static Vector2[] To2(string tex)
    {
        tempUv = Game.TextureRects[tex];

        return new Vector2[] { new Vector2(tempUv.xMin, tempUv.yMin), new Vector2(tempUv.xMin, tempUv.yMax),new Vector2(tempUv.xMax, tempUv.yMax), new Vector2(tempUv.xMax, tempUv.yMin) };
    }
    static Vector2[] To2Rotated(string tex)
    {
        tempUv = Game.TextureRects[tex];

        return new Vector2[] { new Vector2(tempUv.xMin, tempUv.yMax), new Vector2(tempUv.xMax, tempUv.yMax), new Vector2(tempUv.xMin, tempUv.yMin), new Vector2(tempUv.xMax, tempUv.yMin) };
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

    public static implicit operator Block(BlockInfo bi) => bi.Instance();
}

public class BlockInfoMesh : BlockInfo
{
    public BlockInfoMesh() { }
    public BlockInfoMesh(string name, int price = 1, Dictionary<Type, object[]> components = null)
    {
        this.Name = name;
        this.uvs = Game.TextureMeshUvs[name];
        this.IsTransparent = true;
        this.mesh = Game.Meshes[name];
        Price = price;

        Vector3 sizef = mesh.bounds.max - mesh.bounds.min;
        Vector3Int size = new Vector3Int(Mathf.CeilToInt(sizef.x), Mathf.CeilToInt(sizef.y), Mathf.CeilToInt(sizef.z));

        if (size.x > 1 || size.y > 1 || size.z > 1)
        {
            List<Vector3Int> args = new List<Vector3Int>();
            if (components == null) components = new Dictionary<Type, object[]>();

            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                    for (int z = 0; z < size.z; z++)
                        args.Add(new Vector3Int(x, y, z));

            components.Add(typeof(MultiblockComponent), new object[] { args.ToArray() });
        }

        Builder.Blocks.Add(this);
        if (components != null) Components = components;
    }
}