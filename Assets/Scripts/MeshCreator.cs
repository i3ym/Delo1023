using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

static class MeshCreator
{
    static Coroutine updateMeshAfterWait;
    static List<int> tris = new List<int>();
    static List<Vector3> verts = new List<Vector3>();
    static List<Vector2> uv = new List<Vector2>();

    static List<Vector3>[] vertss;
    static List<int>[] triss;
    static List<Vector2>[] uvss;
    static int indx;

    public static void UpdateMeshFast(Chunk c, int x, int y, int z, Mesh mesh, int meshIndex, Block block) //TODO optimize
    {
        if (updateMeshAfterWait != null) Game.game.StopCoroutine(updateMeshAfterWait);

        vertss[x] = new List<Vector3>(mesh.vertices);
        triss[x] = new List<int>(mesh.triangles);
        uvss[x] = new List<Vector2>(mesh.uv);

        AddCube(x, y, z, block.Info.IsTransparent, block.Info.uvs, c, (int _x, int _y, int _z) => c.GetBlock(_x, _y, _z) == null || c.GetBlock(_x, _y, _z).Info.IsTransparent);

        verts = vertss[x];
        tris = triss[x];
        uv = uvss[x];

        DivideMeshesAndSet((sve, eve, str, etr, meshInd) => c.SetMesh(verts.GetRange(sve, eve).ToArray(), tris.GetRange(str, etr).ToArray(), uv.GetRange(sve, eve).ToArray(), meshInd, false), meshIndex);

        vertss[x] = new List<Vector3>(mesh.vertices);
        triss[x] = new List<int>(mesh.triangles);
        uvss[x] = new List<Vector2>();

        AddCube(x, y, z, false, null, c, (int _x, int _y, int _z) => c.GetBlock(_x, _y, _z) == null);

        verts = vertss[x];
        tris = triss[x];
        uv = uvss[x];

        DivideMeshesAndSet((sve, eve, str, etr, meshInd) => c.SetMesh(verts.GetRange(sve, eve).ToArray(), tris.GetRange(str, etr).ToArray(), null, meshInd, true), meshIndex);

        updateMeshAfterWait = Game.game.StartCoroutine(UpdateMeshAfterWait(c));
    }
    static IEnumerator UpdateMeshAfterWait(Chunk c)
    {
        yield return new WaitForSeconds(5f);
        new Thread(() => UpdateMesh(c, c.Blocks, c.sizeY, false)).Start();
    }
    public static void UpdateMesh(Chunk c, List<Block[, ]> Blocks, int sizeY, bool isMainThread = true)
    {
        vertss = new List<Vector3>[Chunk.maxX];
        triss = new List<int>[Chunk.maxX];
        uvss = new List<Vector2>[Chunk.maxX];

        Parallel.For(0, Chunk.maxX, (int x, ParallelLoopState _) =>
        {
            Vector3 coords = new Vector3();
            Block tb;

            vertss[x] = new List<Vector3>();
            triss[x] = new List<int>();
            uvss[x] = new List<Vector2>();

            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < Chunk.maxZ; z++)
                {
                    tb = Blocks[y][x, z];
                    coords.Set(x + .5f, y, z + .5f);

                    if (tb == null) continue;

                    if (tb.Info.mesh != null) AddMesh(x, y, z, tb, c, coords);
                    else AddCube(x, y, z, tb.Info.IsTransparent, tb.Info.uvs, c, (int _x, int _y, int _z) => c.GetBlock(_x, _y, _z) == null || c.GetBlock(_x, _y, _z).Info.IsTransparent);
                }
            }
        });

        CombineArrays();
        DivideMeshesAndSet((sve, eve, str, etr, meshInd) => c.SetMesh(verts.GetRange(sve, eve).ToArray(), tris.GetRange(str, etr).ToArray(), uv.GetRange(sve, eve).ToArray(), meshInd, false, isMainThread));

        // Mesh Collider

        Parallel.For(0, Chunk.maxX, (int x, ParallelLoopState _) =>
        {
            vertss[x] = new List<Vector3>();
            triss[x] = new List<int>();
            uvss[x] = new List<Vector2>();

            for (int y = 0; y < sizeY; y++)
                for (int z = 0; z < Chunk.maxZ; z++)
                    if (Blocks[y][x, z] != null)
                        AddCube(x, y, z, false, null, c, (int _x, int _y, int _z) => c.GetBlock(_x, _y, _z) == null);
        });

        CombineArrays();
        DivideMeshesAndSet((sve, eve, str, etr, meshInd) => c.SetMesh(verts.GetRange(sve, eve).ToArray(), tris.GetRange(str, etr).ToArray(), null, meshInd, true, isMainThread));

        /// Mesh Collider
    }
    static void DivideMeshesAndSet(Action<int, int, int, int, int> setm, int meshIndex = 0)
    {
        int ve, tr;

        for (int i = 0; true; i++)
        {
            ve = i * 65499;
            tr = i * 21883;

            if ((i + 1) * 65499 > verts.Count)
            {
                setm(ve, ve + verts.Count, tr, tr + tris.Count, meshIndex++);
                return;
            }
            else setm(ve, 65499, tr, 21883, meshIndex++);
        }
    }
    static void AddMesh(int x, int y, int z, Block tb, Chunk c, Vector3 coords)
    {
        bool isgl(int _x, int _y, int _z) => c.GetBlock(_x, _y, _z) != null && c.GetBlock(_x, _y, _z).Info == Block.GlassPane;

        bool xb = isgl(x + 1, y, z);
        bool xbn = isgl(x - 1, y, z);
        bool zb = isgl(x, y, z + 1);
        bool zbn = isgl(x, y, z - 1);

        MultimodelComponent mmc = tb.GetComponent<MultimodelComponent>();
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
                    foreach (Vector3 v in mmc.side.vertices) vertss[x].Add(Chunk.angle2 * v + coords);
                else if (!zb)
                    foreach (Vector3 v in mmc.side.vertices) vertss[x].Add(Chunk.angle3 * v + coords);
                else if (!zbn)
                    foreach (Vector3 v in mmc.side.vertices) vertss[x].Add(Chunk.angle1 * v + coords);
            }
            else if ((xb && zb) || (xb && zbn) || (xbn && zb) || (xbn && zbn)) // г
            {
                foreach (int v in mmc.corner.triangles)
                    triss[x].Add(v + vertss[x].Count);
                uvss[x].AddRange(Game.TextureMeshUvs[mmc.corner.name]);

                if (xb && zb)
                    foreach (Vector3 v in mmc.corner.vertices) vertss[x].Add(Chunk.angle1 * v + coords);
                else if (zb && xbn)
                    foreach (Vector3 v in mmc.corner.vertices) vertss[x].Add(v + coords);
                else if (xbn && zbn)
                    foreach (Vector3 v in mmc.corner.vertices) vertss[x].Add(Chunk.angle3 * v + coords);
                else if (zbn && xb)
                    foreach (Vector3 v in mmc.corner.vertices) vertss[x].Add(Chunk.angle2 * v + coords);
            }
            else // -
            {
                foreach (int v in tb.Info.mesh.triangles)
                    triss[x].Add(v + vertss[x].Count);
                uvss[x].AddRange(tb.Info.uvs);

                if (xb || xbn)
                    foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(v + coords);
                else if (zb || zbn)
                    foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(Chunk.angle1 * v + coords);
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
                foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(Chunk.angle1 * v + coords);
            else if (tb.Rotation == 2)
                foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(Chunk.angle2 * v + coords);
            else if (tb.Rotation == 3)
                foreach (Vector3 v in tb.Info.mesh.vertices) vertss[x].Add(Chunk.angle3 * v + coords);
        }
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
        var adds = Chunk.CubeMeshes[side];

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
            uv.Add(uvs[(int) side * 4 + 0]);
            uv.Add(uvs[(int) side * 4 + 1]);
            uv.Add(uvs[(int) side * 4 + 2]);
            uv.Add(uvs[(int) side * 4 + 3]);
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