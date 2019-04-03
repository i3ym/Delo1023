using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk
{
    public const int maxX = 20;
    public const int maxZ = 20;
    public int X, Z;
    public BlockList Blocks = new BlockList();
    public int Price = 0;
    public Building building = null;
    public GameObject parent;
    World world;
    List<Mesh> meshes = new List<Mesh>();
    List<GameObject> meshHolders = new List<GameObject>();

    public static Quaternion angle1, angle2, angle3;
    public static readonly Dictionary < Sides, (sbyte[] X, sbyte[] Y, sbyte[] Z) > CubeMeshes = new Dictionary < Sides, (sbyte[] X, sbyte[] Y, sbyte[] Z) > ();

    static Chunk()
    {
        angle1 = Quaternion.AngleAxis(90f, Vector3.up);
        angle2 = Quaternion.AngleAxis(180f, Vector3.up);
        angle3 = Quaternion.AngleAxis(270f, Vector3.up);

        CubeMeshes.Add(Sides.Right, (new sbyte[] { 1, 1, 1, 1 }, new sbyte[] { 0, 1, 1, 0 }, new sbyte[] { 0, 0, 1, 1 }));
        CubeMeshes.Add(Sides.Left, (new sbyte[] { 0, 0, 0, 0 }, new sbyte[] { 0, 1, 1, 0 }, new sbyte[] { 1, 1, 0, 0 }));

        CubeMeshes.Add(Sides.Top, (new sbyte[] { 0, 0, 1, 1 }, new sbyte[] { 1, 1, 1, 1 }, new sbyte[] { 0, 1, 1, 0 }));
        CubeMeshes.Add(Sides.Bottom, (new sbyte[] { 1, 1, 0, 0 }, new sbyte[] { 0, 0, 0, 0 }, new sbyte[] { 0, 1, 1, 0 }));

        CubeMeshes.Add(Sides.Front, (new sbyte[] { 1, 1, 0, 0 }, new sbyte[] { 0, 1, 1, 0 }, new sbyte[] { 1, 1, 1, 1 }));
        CubeMeshes.Add(Sides.Back, (new sbyte[] { 0, 0, 1, 1 }, new sbyte[] { 0, 1, 1, 0 }, new sbyte[] { 0, 0, 0, 0 }));
    }
    public Chunk(int x, int z, World w)
    {
        X = x;
        Z = z;
        world = w;

        parent = new GameObject(x + ", " + z);
        parent.transform.position = new Vector3(x * maxX, 0, z * maxX);

        Generate();
        MeshCreator.UpdateMesh(this, Blocks);
    }

    public void Generate()
    {
        const float seed = .5f;

        Blocks.Clear();
        Game.Money += Price;
        Price = 0;

        Parallel.For(0, maxX, (int xx, ParallelLoopState _) =>
        {
            int perlin;
            for (int zz = 0; zz < maxZ; zz++)
            {
                perlin = (int) (Perlin.Noise((xx + (X * maxX)) / (float) (maxX * World.sizeX), (zz + (Z * maxZ)) / (float) (maxZ * World.sizeZ)) * 3f) + 2;
                perlin += (int) (Perlin.Noise(Mathf.Pow(xx + (X * maxX), seed), Mathf.Pow(zz + (Z * maxZ), seed)) * 3f);
                perlin = Math.Max(perlin, 0) + 30;

                for (int yy = 0; yy < perlin; yy++)
                    SetBlock(xx, yy, zz, Block.Dirt.Instance(), rotation : 0, takeMoney : false);

                SetBlock(xx, perlin, zz, Block.Grass.Instance(), rotation : 0, takeMoney : false);
            }
        });
    }
    public int CalculatePrice()
    {
        int price = 0;

        foreach (Block b in Blocks)
            if (b != null) price += b.Info.Price;

        return price;
    }
    public bool SetBlock(int x, int y, int z, Block b, byte rotation = byte.MaxValue, bool takeMoney = true)
    {
        if (Blocks.GetBlock(x, y, z) != null) return false;

        if (b.Info.mesh != null && b.Info != Block.Transparent)
        {
            if (rotation != byte.MaxValue) b.Rotation = rotation;
            else
            {
                b.Rotation = (byte) ((Game.camera.transform.eulerAngles.y + 45f) / 90);
                if (b.Rotation == 4) b.Rotation = 0;
            }

            MultiblockComponent mc = b.GetComponent<MultiblockComponent>();
            if (mc != null)
                for (int i = 0; i < mc.Locations.Length; i++)
                {
                    if (b.Rotation == 1) mc.Locations[i].Set(mc.Locations[i].z, mc.Locations[i].y, -mc.Locations[i].x);
                    else if (b.Rotation == 2) mc.Locations[i].Set(-mc.Locations[i].x, mc.Locations[i].y, -mc.Locations[i].z);
                    else if (b.Rotation == 3) mc.Locations[i].Set(-mc.Locations[i].z, mc.Locations[i].y, mc.Locations[i].x);
                }
        }

        if (takeMoney && Game.Money < b.Info.Price) return false;

        if (b.OnPlace(x + X * maxX, y, z + Z * maxZ, b.Rotation))
        {
            Blocks.SetBlock(x, y, z, b);
            if (takeMoney) Game.Money -= b.Info.Price;
            Price += b.Info.Price;
            world.UpdateChunk(this);

            return true;
        }

        return false;
    }
    public void RemoveBlock(int x, int y, int z, bool shootEvent = true, bool takeMoney = true)
    {
        if (IsCoordsOffBounds(x, y, z)) return;

        Block block = Blocks.GetBlock(x, y, z);

        if (shootEvent)
        {
            if (block.OnBreak(x + X * maxX, y, z + Z * maxZ))
            {
                if (takeMoney && block != null) Game.Money += block.Info.Price;
                Blocks.SetBlock(x, y, z, null);
            }
        }
        else
        {
            if (takeMoney && block != null) Game.Money += block.Info.Price;
            Blocks.SetBlock(x, y, z, null);
        }
        Price -= block.Info.Price;

        world.UpdateChunk(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block GetBlock(int x, int y, int z) => IsCoordsOffBounds(x, y, z) ? null : Blocks.GetBlock(x, y, z);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool IsCoordsOffBounds(int x, int y, int z) => y < 0 || x < 0 || x > maxX - 1 || z < 0 || z > maxX - 1;
    public void SetMesh(Vector3[] verts, int[] tris, Vector2[] uv, int index, bool isMainThread = true)
    {
        if (!isMainThread)
        {
            Game.Invoke(() => SetMesh(verts, tris, uv, index));
            return;
        }

        Mesh mesh = new Mesh();
        GameObject go;

        if (meshes.Count <= index)
        {
            mesh = new Mesh();
            meshes.Add(mesh);
        }
        else mesh = meshes[index];

        if (meshHolders.Count <= index)
        {
            go = new GameObject();
            go.AddComponent<MeshFilter>().mesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = Game.material;
            mr.shadowCastingMode = ShadowCastingMode.On;

            go.transform.SetParent(parent.transform);
            go.transform.position = parent.transform.position;
            go.layer = 10;

            meshHolders.Add(go);
        }
        else go = meshHolders[index];

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        go.GetComponent<MeshFilter>().mesh = mesh;
    }
}
public enum Sides
{
    Right,
    Left,
    Top,
    Bottom,
    Front,
    Back
}