using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk
{
    public const int maxX = 20;
    public const int maxZ = 20;
    public int X, Z;
    public List<Block[, ]> Blocks = new List<Block[, ]>();
    public int Price = 0;
    public Building building = null;
    public int sizeY = 0;
    World world;
    public GameObject parent;
    List<Mesh> meshes = new List<Mesh>();
    List<GameObject> meshHolders = new List<GameObject>();
    readonly Vector3 zero = Vector3.zero;
    Thread meshUpdateThread;

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
        MeshCreator.UpdateMesh(this, Blocks, sizeY);
    }

    void Generate()
    {
        for (int xx = 0; xx < maxX; xx++)
            for (int yy = 0; yy < 9; yy++)
                for (int zz = 0; zz < maxZ; zz++)
                    SetBlock(xx, yy, zz, Block.Dirt.Instance(), false);
        for (int xx = 0; xx < maxX; xx++)
            for (int zz = 0; zz < maxZ; zz++)
                SetBlock(xx, 9, zz, Block.Grass.Instance(), false);
    }
    public int CalculatePrice()
    {
        int price = 0;

        for (int xx = 0; xx < maxX; xx++)
            for (int yy = 0; yy < 10; yy++)
                for (int zz = 0; zz < maxZ; zz++)
                    if (Blocks[yy][xx, zz] != null)
                        price += Blocks[yy][xx, zz].Info.Price;

        return price;
    }
    public bool SetBlock(int x, int y, int z, Block b, bool update = true, bool updateFast = false)
    {
        while (y >= sizeY)
        {
            Blocks.Add(new Block[maxX, maxZ]);
            sizeY++;
        }

        if (Blocks[y][x, z] != null) return false;

        if (b.Info != Block.Transparent && b.Info.mesh != null)
        {
            b.Rotation = (byte) ((Game.camera.transform.eulerAngles.y + 45f) / 90);
            if (b.Rotation == 4) b.Rotation = 0;

            MultiblockComponent mc = b.GetComponent<MultiblockComponent>();
            if (mc != null)
                for (int i = 0; i < mc.Locations.Length; i++)
                {
                    if (b.Rotation == 1) mc.Locations[i].Set(mc.Locations[i].z, mc.Locations[i].y, -mc.Locations[i].x);
                    else if (b.Rotation == 2) mc.Locations[i].Set(-mc.Locations[i].x, mc.Locations[i].y, -mc.Locations[i].z);
                    else if (b.Rotation == 3) mc.Locations[i].Set(-mc.Locations[i].z, mc.Locations[i].y, mc.Locations[i].x);
                }
        }

        if (b.OnPlace(x + X * maxX, y, z + Z * maxZ, b.Rotation))
        {
            Blocks[y][x, z] = b;

            if (update)
            {
                if (updateFast)
                {
                    if (meshUpdateThread != null && meshUpdateThread.IsAlive) meshUpdateThread.Interrupt();

                    MeshCreator.UpdateMeshFast(this, x, y, z, meshes[meshes.Count - 1], meshes.Count - 1, b);
                    meshUpdateThread = new Thread(() => MeshCreator.UpdateMesh(this, Blocks, sizeY, false));
                    meshUpdateThread.Start();
                }
                else MeshCreator.UpdateMesh(this, Blocks, sizeY);
            }
            return true;
        }

        return false;
    }
    public Block[] GetBlocksArray()
    {
        List<Block> blocks = new List<Block>();

        foreach (Block[, ] b in Blocks)
            foreach (Block bb in b)
                blocks.Add(bb);

        return blocks.ToArray();
    }
    public void RemoveBlock(int x, int y, int z, bool shootEvent = true)
    {
        if (y < 0 || y >= sizeY) return;
        if (x < 0 || x > maxX - 1 || z < 0 || z > maxX - 1) return;

        if (shootEvent && Blocks[y][x, z].OnBreak(x + X * maxX, y, z + Z * maxZ)) Blocks[y][x, z] = null;
        else Blocks[y][x, z] = null;
        MeshCreator.UpdateMesh(this, Blocks, sizeY);
    }
    public Block GetBlock(int x, int y, int z)
    {
        if (y < 0 || y >= sizeY) return null;
        if (x < 0 || x > maxX - 1 || z < 0 || z > maxX - 1) return null;

        return Blocks[y][x, z];
    }
    public void SetMesh(Vector3[] verts, int[] tris, Vector2[] uv, int index, bool isCollider, bool isMainThread = true)
    {
        if (!isMainThread)
        {
            world.Invoke(() => SetMesh(verts, tris, uv, index, isCollider));
            return;
        }

        Mesh mesh;
        GameObject go;

        if (meshes.Count <= index)
        {
            mesh = new Mesh();
            go = new GameObject();

            if (isCollider) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            else
            {
                go.AddComponent<MeshFilter>().mesh = mesh;
                go.AddComponent<MeshRenderer>().material = Game.material;
                go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;
            }

            go.transform.SetParent(parent.transform);
            go.transform.localPosition = zero;
            go.layer = 10;

            meshes.Add(mesh);
            meshHolders.Add(go);
        }
        else
        {
            if (!isCollider) mesh = meshes[index];
            else mesh = new Mesh();

            go = meshHolders[index];
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;

        if (!isCollider)
        {
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        mesh.Optimize(); //TODO what is this

        if (isCollider)
        {
            MeshCollider mc = go.GetComponent<MeshCollider>();
            if (mc == null) mc = go.AddComponent<MeshCollider>();

            mc.sharedMesh = mesh;
        }
        else
        {
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.GetComponent<MeshRenderer>().material = Game.material;
        }
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