using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public const int maxX = 20;
    public const int maxZ = 20;
    public int X, Z;
    public List<Block[, ]> Blocks = new List<Block[, ]>();
    public int Price = 0;
    public Building building = null;
    int sizeY = 0;
    World world;
    public GameObject parent;
    List<Mesh> meshes = new List<Mesh>();
    List<GameObject> meshHolders = new List<GameObject>();
    readonly Vector3 zero = Vector3.zero;

    static Quaternion angle1, angle2, angle3;

    List<int> tris = new List<int>();
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int> trisM = new List<int>();
    List<Vector3> vertsM = new List<Vector3>();
    List<Vector2> uvM = new List<Vector2>();

    public Chunk(int x, int z, World w)
    {
        X = x;
        Z = z;
        world = w;

        angle1 = Quaternion.AngleAxis(90f, Vector3.up);
        angle2 = Quaternion.AngleAxis(180f, Vector3.up);
        angle3 = Quaternion.AngleAxis(270f, Vector3.up);

        parent = new GameObject(x + ", " + z);
        parent.transform.position = new Vector3(x * maxX, 0, z * maxX);

        for (int xx = 0; xx < maxX; xx++)
            for (int yy = 0; yy < 9; yy++)
                for (int zz = 0; zz < maxZ; zz++)
                    SetBlock(xx, yy, zz, Block.Dirt.Instance(), false);
        for (int xx = 0; xx < maxX; xx++)
            for (int zz = 0; zz < maxZ; zz++)
                SetBlock(xx, 9, zz, Block.Grass.Instance(), false);

        UpdateMesh();
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
            if (b.Rotation == 5) b.Rotation = 0;

            MultiblockComponent mc = b.GetComponent<MultiblockComponent>();
            if (mc != null)
                for (int i = 0; i < mc.Locations.Length; i++)
                {
                    if (b.Rotation == 1) mc.Locations[i].Set(mc.Locations[i].z, mc.Locations[i].y, -mc.Locations[i].x);
                    else if (b.Rotation == 2) mc.Locations[i].Set(-mc.Locations[i].x, mc.Locations[i].y, -mc.Locations[i].z);
                    else if (b.Rotation == 3) mc.Locations[i].Set(-mc.Locations[i].z, mc.Locations[i].y, mc.Locations[i].x);
                }
        }

        if (b.OnPlace(x + X * maxX, y, z + Z * maxZ))
        {
            Blocks[y][x, z] = b;

            if (update) UpdateMesh();
            return true;
        }

        return false;
    }
    public void RemoveBlock(int x, int y, int z, bool shootEvent = true)
    {
        if (y < 0 || y >= sizeY) return;
        if (x < 0 || x > maxX - 1 || z < 0 || z > maxX - 1) return;

        if (shootEvent && Blocks[y][x, z].OnBreak(x + X * maxX, y, z + Z * maxZ)) Blocks[y][x, z] = null;
        else Blocks[y][x, z] = null;
        UpdateMesh();
    }
    public Block GetBlock(int x, int y, int z)
    {
        if (y < 0 || y >= sizeY) return null;
        if (x < 0 || x > maxX - 1 || z < 0 || z > maxX - 1) return null; //TODO ? return world.GetBlock(x * maxX, y, z * maxZ);

        return Blocks[y][x, z];
    }
    void UpdateMesh()
    {
        tris.Clear();
        verts.Clear();
        uv.Clear();
        trisM.Clear();
        vertsM.Clear();
        uvM.Clear();

        Block tb;
        int meshIndex = 0;
        Vector3 coords = new Vector3();

        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    if (verts.Count > 65300) SetMesh(verts.ToArray(), tris.ToArray(), uv.ToArray(), meshIndex++);
                    if (vertsM.Count > 64000) SetMesh(vertsM.ToArray(), trisM.ToArray(), uvM.ToArray(), meshIndex++, true);

                    tb = Blocks[y][x, z];

                    if (tb == null) continue;

                    if (tb.Info.mesh != null)
                    {
                        foreach (int v in tb.Info.mesh.triangles)
                            trisM.Add(v + vertsM.Count);
                        uvM.AddRange(tb.Info.uvs);
                        
                        coords.Set(x + .5f, y, z + .5f);

                        switch (tb.Rotation)
                        {
                            case 0:
                                foreach (Vector3 v in tb.Info.mesh.vertices) vertsM.Add(v + coords);
                                break;
                            case 1:
                                foreach (Vector3 v in tb.Info.mesh.vertices) vertsM.Add(angle1 * v + coords);
                                break;
                            case 2:
                                foreach (Vector3 v in tb.Info.mesh.vertices) vertsM.Add(angle2 * v + coords);
                                break;
                            case 3:
                                foreach (Vector3 v in tb.Info.mesh.vertices) vertsM.Add(angle3 * v + coords);
                                break;
                        }
                    }
                    else
                    {
                        if (tb.Info.IsTransparent) AddCubeMesh(true, true, true, true, true, true, x, y, z, tb.Info.uvs);
                        else AddCubeMesh(istr(x, y, z - 1), istr(x, y, z + 1), istr(x, y + 1, z), istr(x, y - 1, z), istr(x + 1, y, z), istr(x - 1, y, z), x, y, z, tb.Info.uvs);
                    }
                }
            }
        }
        bool istr(int _x, int _y, int _z) => GetBlock(_x, _y, _z) == null || GetBlock(_x, _y, _z).Info.IsTransparent;
        bool isbl(int _x, int _y, int _z) => GetBlock(_x, _y, _z) == null;

        SetMesh(verts.ToArray(), tris.ToArray(), uv.ToArray(), meshIndex++);
        SetMesh(vertsM.ToArray(), trisM.ToArray(), uvM.ToArray(), meshIndex++, true);

        tris.Clear();
        verts.Clear();
        uv.Clear();
        int index = 0;
        Mesh mesh;

        for (int x = 0; x < maxX; x++)
            for (int y = 0; y < sizeY; y++)
                for (int z = 0; z < maxZ; z++)
                {
                    if (verts.Count > 65300)
                    {
                        mesh = new Mesh();
                        mesh.vertices = verts.ToArray();
                        mesh.triangles = tris.ToArray();
                        mesh.uv = uv.ToArray();
                        mesh.RecalculateNormals();
                        meshHolders[index++].GetComponent<MeshCollider>().sharedMesh = mesh;

                        tris.Clear();
                        verts.Clear();
                        uv.Clear();
                    }

                    if (!isbl(x, y, z))
                        AddCubeMesh(isbl(x, y, z - 1), isbl(x, y, z + 1), isbl(x, y + 1, z), isbl(x, y - 1, z), isbl(x + 1, y, z), isbl(x - 1, y, z), x, y, z, null);
                }

        mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();
        meshHolders[index].GetComponent<MeshCollider>().sharedMesh = mesh;

        for (int i = meshIndex; i < meshes.Count; i++)
        {
            meshes[i].Clear();
            meshHolders[i].GetComponent<MeshFilter>().mesh = meshes[i];
        }
    }
    void AddCubeMesh(bool l, bool r, bool t, bool bo, bool f, bool ba, int x, int y, int z, Vector2[] uvs)
    {
        int index;

        #region left
        if (l) // lft
        {
            index = verts.Count;
            verts.Add(new Vector3(x, y, z)); //0
            verts.Add(new Vector3(x, y + 1, z)); //1
            verts.Add(new Vector3(x + 1, y, z)); //2
            verts.Add(new Vector3(x + 1, y + 1, z)); //3
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 3);
            tris.Add(index + 0);
            tris.Add(index + 3);
            tris.Add(index + 2);
            if (uvs != null)
            {
                uv.Add(uvs[0]);
                uv.Add(uvs[1]);
                uv.Add(uvs[2]);
                uv.Add(uvs[3]);
            }
        }
        #endregion
        #region right
        if (r)
        {
            index = verts.Count;
            verts.Add(new Vector3(x, y, z + 1)); //0
            verts.Add(new Vector3(x, y + 1, z + 1)); //1
            verts.Add(new Vector3(x + 1, y, z + 1)); //2
            verts.Add(new Vector3(x + 1, y + 1, z + 1)); //3
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 1);
            tris.Add(index + 0);
            if (uvs != null)
            {
                uv.Add(uvs[4]);
                uv.Add(uvs[5]);
                uv.Add(uvs[6]);
                uv.Add(uvs[7]);
            }
        }
        #endregion
        #region top
        if (t)
        {
            index = verts.Count;
            verts.Add(new Vector3(x, y + 1, z)); //0
            verts.Add(new Vector3(x, y + 1, z + 1)); //1
            verts.Add(new Vector3(x + 1, y + 1, z)); //2
            verts.Add(new Vector3(x + 1, y + 1, z + 1)); //3
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 3);
            tris.Add(index + 0);
            tris.Add(index + 3);
            tris.Add(index + 2);
            if (uvs != null)
            {
                uv.Add(uvs[8]);
                uv.Add(uvs[9]);
                uv.Add(uvs[10]);
                uv.Add(uvs[11]);
            }
        }
        #endregion
        #region bottom
        if (bo)
        {
            index = verts.Count;
            verts.Add(new Vector3(x, y, z)); //0
            verts.Add(new Vector3(x, y, z + 1)); //1
            verts.Add(new Vector3(x + 1, y, z)); //2
            verts.Add(new Vector3(x + 1, y, z + 1)); //3
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 1);
            tris.Add(index + 0);
            if (uvs != null)
            {
                uv.Add(uvs[12]);
                uv.Add(uvs[13]);
                uv.Add(uvs[14]);
                uv.Add(uvs[15]);
            }
        }
        #endregion
        #region front
        if (f)
        {
            index = verts.Count;
            verts.Add(new Vector3(x + 1, y, z)); //0
            verts.Add(new Vector3(x + 1, y, z + 1)); //1
            verts.Add(new Vector3(x + 1, y + 1, z)); //2
            verts.Add(new Vector3(x + 1, y + 1, z + 1)); //3
            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
            tris.Add(index + 3);
            tris.Add(index + 1);
            if (uvs != null)
            {
                uv.Add(uvs[16]);
                uv.Add(uvs[17]);
                uv.Add(uvs[18]);
                uv.Add(uvs[19]);
            }
        }
        #endregion
        #region back
        if (ba)
        {
            index = verts.Count;
            verts.Add(new Vector3(x, y, z)); //0
            verts.Add(new Vector3(x, y, z + 1)); //1
            verts.Add(new Vector3(x, y + 1, z)); //2
            verts.Add(new Vector3(x, y + 1, z + 1)); //3
            tris.Add(index + 1);
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 0);
            if (uvs != null)
            {
                uv.Add(uvs[20]);
                uv.Add(uvs[21]);
                uv.Add(uvs[22]);
                uv.Add(uvs[23]);
            }
        }
        #endregion
    }
    void SetMesh(Vector3[] verts, int[] tris, Vector2[] uv, int index, bool meshTexture = false)
    {
        Mesh mesh;
        GameObject go;

        if (meshes.Count <= index)
        {
            mesh = new Mesh();
            go = new GameObject();

            go.AddComponent<MeshFilter>().mesh = mesh;
            if (!meshTexture) go.AddComponent<MeshRenderer>().material = Game.material;
            else go.AddComponent<MeshRenderer>().material = Game.materialMesh;
            go.AddComponent<MeshCollider>();
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = zero;
            go.layer = 10;

            meshes.Add(mesh);
            meshHolders.Add(go);
        }
        else
        {
            mesh = meshes[index];
            go = meshHolders[index];
        }

        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        go.GetComponent<MeshFilter>().mesh = mesh;
        if (!meshTexture) go.GetComponent<MeshRenderer>().material = Game.material;
        else go.GetComponent<MeshRenderer>().material = Game.materialMesh;
    }
}