﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

    public static Quaternion angle1, angle2, angle3;
    static Dictionary < Sides, (sbyte[] X, sbyte[] Y, sbyte[] Z) > CubeMeshes = new Dictionary < Sides, (sbyte[] X, sbyte[] Y, sbyte[] Z) > ();

    static Chunk()
    {
        angle1 = Quaternion.AngleAxis(90f, Vector3.up);
        angle2 = Quaternion.AngleAxis(180f, Vector3.up);
        angle3 = Quaternion.AngleAxis(270f, Vector3.up);

        CubeMeshes.Add(Sides.Front, (new sbyte[] { 1, 1, 0, 0 }, new sbyte[] { 0, 1, 1, 0 }, new sbyte[] { 1, 1, 1, 1 }));
        CubeMeshes.Add(Sides.Back, (new sbyte[] { 0, 0, 1, 1 }, new sbyte[] { 0, 1, 1, 0 }, new sbyte[] { 0, 0, 0, 0 }));

        CubeMeshes.Add(Sides.Top, (new sbyte[] { 0, 0, 1, 1 }, new sbyte[] { 1, 1, 1, 1 }, new sbyte[] { 0, 1, 1, 0 }));
        CubeMeshes.Add(Sides.Bottom, (new sbyte[] { 1, 1, 0, 0 }, new sbyte[] { 0, 0, 0, 0 }, new sbyte[] { 0, 1, 1, 0 }));

        CubeMeshes.Add(Sides.Right, (new sbyte[] { 1, 1, 1, 1 }, new sbyte[] { 0, 1, 1, 0 }, new sbyte[] { 0, 0, 1, 1 }));
        CubeMeshes.Add(Sides.Left, (new sbyte[] { 0, 0, 0, 0 }, new sbyte[] { 0, 1, 1, 0 }, new sbyte[] { 1, 1, 0, 0 }));
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
    public bool SetBlock(int x, int y, int z, Block b, bool update = true)
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

            if (update) MeshCreator.UpdateMesh(this, Blocks, sizeY);
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
        if (x < 0 || x > maxX - 1 || z < 0 || z > maxX - 1) return null; //TODO ? return world.GetBlock(x * maxX, y, z * maxZ);

        return Blocks[y][x, z];
    }
    void SetMesh(Vector3[] verts, int[] tris, Vector2[] uv, int index, bool isCollider)
    {
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

    static class MeshCreator
    {
        static List<int> tris = new List<int>();
        static List<Vector3> verts = new List<Vector3>();
        static List<Vector2> uv = new List<Vector2>();

        static List<Vector3>[] vertss;
        static List<int>[] triss;
        static List<Vector2>[] uvss;
        static int indx;

        public static void UpdateMesh(Chunk c, List<Block[, ]> Blocks, int sizeY)
        {
            vertss = new List<Vector3>[maxX];
            triss = new List<int>[maxX];
            uvss = new List<Vector2>[maxX];

            Parallel.For(0, maxX, (int x, ParallelLoopState _) =>
            {
                Vector3 coords = new Vector3();
                bool xb, xbn, zb, zbn;
                MultimodelComponent mmc;
                Block tb;

                vertss[x] = new List<Vector3>();
                triss[x] = new List<int>();
                uvss[x] = new List<Vector2>();

                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {
                        tb = Blocks[y][x, z];
                        coords.Set(x + .5f, y, z + .5f);

                        if (tb == null) continue;

                        if (tb.Info.mesh != null)
                        {
                            xb = isgl(x + 1, y, z);
                            xbn = isgl(x - 1, y, z);
                            zb = isgl(x, y, z + 1);
                            zbn = isgl(x, y, z - 1);

                            mmc = tb.GetComponent<MultimodelComponent>();
                            if (mmc != null && (xb || xbn || zb || zbn))
                            {
                                if (xb && xbn && zb && zbn) // +
                                {
                                    foreach (int v in mmc.center.triangles)
                                        triss[x].Add(v + vertss[x].Count);
                                    uvss[x].AddRange(Game.TextureMeshUvs[mmc.center.name]);
                                    foreach (Vector3 v in mmc.center.vertices) vertss[x].Add(v + coords);
                                }
                                else if ((xb && xbn && zb) || (xb && xbn && zbn) || (xb && zb && zbn) || (xbn && zb && zbn)) // т
                                {
                                    foreach (int v in mmc.side.triangles)
                                        triss[x].Add(v + vertss[x].Count);
                                    uvss[x].AddRange(Game.TextureMeshUvs[mmc.side.name]);

                                    if (!xb)
                                        foreach (Vector3 v in mmc.side.vertices) vertss[x].Add(v + coords);
                                    else if (!xbn)
                                        foreach (Vector3 v in mmc.side.vertices) vertss[x].Add(angle2 * v + coords);
                                    else if (!zb)
                                        foreach (Vector3 v in mmc.side.vertices) vertss[x].Add(angle3 * v + coords);
                                    else if (!zbn)
                                        foreach (Vector3 v in mmc.side.vertices) vertss[x].Add(angle1 * v + coords);
                                }
                                else if ((xb && zb) || (xb && zbn) || (xbn && zb) || (xbn && zbn)) // г
                                {
                                    foreach (int v in mmc.corner.triangles)
                                        triss[x].Add(v + vertss[x].Count);
                                    uvss[x].AddRange(Game.TextureMeshUvs[mmc.corner.name]);

                                    if (xb && zb)
                                        foreach (Vector3 v in mmc.corner.vertices) vertss[x].Add(angle1 * v + coords);
                                    else if (zb && xbn)
                                        foreach (Vector3 v in mmc.corner.vertices) vertss[x].Add(v + coords);
                                    else if (xbn && zbn)
                                        foreach (Vector3 v in mmc.corner.vertices) vertss[x].Add(angle3 * v + coords);
                                    else if (zbn && xb)
                                        foreach (Vector3 v in mmc.corner.vertices) vertss[x].Add(angle2 * v + coords);
                                }
                                else // -
                                {
                                    foreach (int v in tb.Info.mesh.triangles)
                                        triss[x].Add(v + vertss[x].Count);
                                    uvss[x].AddRange(tb.Info.uvs);

                                    if (xb || xbn)
                                        foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(v + coords);
                                    else if (zb || zbn)
                                        foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(angle1 * v + coords);
                                }
                            }
                            else
                            {
                                foreach (int v in tb.Info.mesh.triangles)
                                    triss[x].Add(v + vertss[x].Count);

                                uvss[x].AddRange(tb.Info.uvs);

                                if (tb.Rotation == 0)
                                    foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(v + coords);
                                else if (tb.Rotation == 1)
                                    foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(angle1 * v + coords);
                                else if (tb.Rotation == 2)
                                    foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(angle2 * v + coords);
                                else if (tb.Rotation == 3)
                                    foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(angle3 * v + coords);
                            }
                        }
                        else AddCube(x, y, z, tb.Info.IsTransparent, tb.Info.uvs, c, (int _x, int _y, int _z) => c.GetBlock(_x, _y, _z) == null || c.GetBlock(_x, _y, _z).Info.IsTransparent);
                    }
                }
            });

            int meshIndex = 0;
            bool isgl(int _x, int _y, int _z) => c.GetBlock(_x, _y, _z) != null && c.GetBlock(_x, _y, _z).Info == Block.GlassPane;
            void setm(int sve, int eve, int str, int etr) => c.SetMesh(verts.GetRange(sve, eve).ToArray(), tris.GetRange(str, etr).ToArray(), uv.GetRange(sve, eve).ToArray(), meshIndex++, false);
            void setmc(int sve, int eve, int str, int etr) => c.SetMesh(verts.GetRange(sve, eve).ToArray(), tris.GetRange(str, etr).ToArray(), null, meshIndex++, true);

            CombineArrays();

            int ve, tr;
            for (int i = 0; true; i++)
            {
                ve = i * 65499;
                tr = i * 21883;

                if ((i + 1) * 65499 > verts.Count)
                {
                    setm(ve, ve + verts.Count, tr, tr + tris.Count);
                    break;
                }
                else setm(ve, 65499, tr, 21883);
            }

            // Mesh Collider

            Parallel.For(0, maxX, (int x, ParallelLoopState _) =>
            {
                vertss[x] = new List<Vector3>();
                triss[x] = new List<int>();
                uvss[x] = new List<Vector2>();

                for (int y = 0; y < sizeY; y++)
                    for (int z = 0; z < maxZ; z++)
                        if (Blocks[y][x, z] != null)
                            AddCube(x, y, z, false, null, c, (int _x, int _y, int _z) => c.GetBlock(_x, _y, _z) == null);
            });

            CombineArrays();

            meshIndex = 0;
            for (int i = 0; true; i++)
            {
                ve = i * 65499;
                tr = i * 21883;

                if ((i + 1) * 65499 > verts.Count)
                {
                    setmc(ve, ve + verts.Count, tr, tr + tris.Count);
                    break;
                }
                else setmc(ve, 65499, tr, 21883);
            }

            /// Mesh Collider
        }
        static void AddCube(int x, int y, int z, bool isTransparent, Vector2[] uvs, Chunk c, Func<int, int, int, bool> isBlockFunc)
        {
            if (isTransparent)
            {
                AddCubeMesh(Sides.Left, x, y, z, uvs, vertss[x], triss[x], uvss[x]);
                AddCubeMesh(Sides.Right, x, y, z, uvs, vertss[x], triss[x], uvss[x]);

                AddCubeMesh(Sides.Top, x, y, z, uvs, vertss[x], triss[x], uvss[x]);
                AddCubeMesh(Sides.Bottom, x, y, z, uvs, vertss[x], triss[x], uvss[x]);

                AddCubeMesh(Sides.Front, x, y, z, uvs, vertss[x], triss[x], uvss[x]);
                AddCubeMesh(Sides.Back, x, y, z, uvs, vertss[x], triss[x], uvss[x]);
            }
            else
            {
                if (isBlockFunc(x, y, z + 1)) AddCubeMesh(Sides.Front, x, y, z, uvs, vertss[x], triss[x], uvss[x]);
                if (isBlockFunc(x, y, z - 1)) AddCubeMesh(Sides.Back, x, y, z, uvs, vertss[x], triss[x], uvss[x]);

                if (isBlockFunc(x, y + 1, z)) AddCubeMesh(Sides.Top, x, y, z, uvs, vertss[x], triss[x], uvss[x]);
                if (isBlockFunc(x, y - 1, z)) AddCubeMesh(Sides.Bottom, x, y, z, uvs, vertss[x], triss[x], uvss[x]);

                if (isBlockFunc(x + 1, y, z)) AddCubeMesh(Sides.Right, x, y, z, uvs, vertss[x], triss[x], uvss[x]);
                if (isBlockFunc(x - 1, y, z)) AddCubeMesh(Sides.Left, x, y, z, uvs, vertss[x], triss[x], uvss[x]);
            }
        }
        static void AddCubeMesh(Sides side, int x, int y, int z, Vector2[] uvs, List<Vector3> verts, List<int> tris, List<Vector2> uv)
        {
            int index = verts.Count;
            var adds = CubeMeshes[side];

            verts.Add(new Vector3(x + adds.X[0], y + adds.Y[0], z + adds.Z[0]));
            verts.Add(new Vector3(x + adds.X[1], y + adds.Y[1], z + adds.Z[1]));
            verts.Add(new Vector3(x + adds.X[2], y + adds.Y[2], z + adds.Z[2]));
            verts.Add(new Vector3(x + adds.X[3], y + adds.Y[3], z + adds.Z[3]));

            tris.Add(index);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index);
            tris.Add(index + 2);
            tris.Add(index + 3);

            if (uvs != null)
            {
                uv.Add(uvs[0]);
                uv.Add(uvs[1]);
                uv.Add(uvs[2]);
                uv.Add(uvs[3]);
            }
        }
        static void CombineArrays()
        {
            tris.Clear();
            verts.Clear();
            uv.Clear();

            for (int i = 0; i < vertss.Length; i++)
            {
                indx = verts.Count;
                verts.AddRange(vertss[i]);
                uv.AddRange(uvss[i]);

                for (int j = 0; j < triss[i].Count; j += 3)
                {
                    tris.Add(triss[i][j] + indx);
                    tris.Add(triss[i][j + 1] + indx);
                    tris.Add(triss[i][j + 2] + indx);
                }
            }
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