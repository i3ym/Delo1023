using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public static int sizeX = 1;
    public static int sizeZ = 1;

    [HideInInspector]
    public Builder builder;
    static Chunk[, ] Chunks = new Chunk[sizeX, sizeZ];
    static List<Chunk> SelectedChunks = new List<Chunk>();
    static RaycastHit hit;
    static int layerMask;
    new static Transform camera;
    static System.Random rnd = new System.Random();
    static List<Action> Actions = new List<Action>();
    static bool invoke = false;
    static List<Chunk> ToUpdate = new List<Chunk>();
    static bool UpdateChunks = false;

    Chunk tempchunk;

    void Start()
    {
        Game.world = this;
        camera = Game.camera.transform;

        builder = gameObject.GetComponent<Builder>();
        layerMask = LayerMask.GetMask("Chunk");

        StartCoroutine(AddVillagersCoroutine());
        StartCoroutine(CreateChunksCoroutine());
        StartCoroutine(SelectChunkCoroutine());
    }
    void Update()
    {
        if (invoke)
        {
            invoke = false;
            foreach (Action action in Actions) action();
        }

        if (UpdateChunks)
        {
            UpdateChunks = false;
            foreach (Chunk c in ToUpdate)
                MeshCreator.UpdateMesh(c, c.Blocks);
        }

        if (Game.Building) return;
    }

    IEnumerator AddVillagersCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(rnd.Next(3, 18 - Math.Min(100, Game.Level) * 15)); //TODO dont forget to 300 and 1800

            if (Game.Villagers < Game.VillagersMax) Game.Villagers++;
        }
    }
    IEnumerator CreateChunksCoroutine()
    {
        for (int x = 0; x < sizeX; x++)
            for (int z = 0; z < sizeZ; z++)
            {
                Chunks[x, z] = new Chunk(x, z, this);
                yield return null;
            }
    }
    IEnumerator SelectChunkCoroutine()
    {
        Vector3 startPos;
        bool exit;
        float time;
        WaitUntil wait = new WaitUntil(() => Input.GetMouseButtonDown(0) && !Game.Building);

        while (true)
        {
            yield return wait;

            exit = false;
            time = Time.time;

            startPos = Input.mousePosition;
            while (Input.GetMouseButton(0))
            {
                if ((Input.mousePosition - startPos).magnitude > 10f)
                {
                    exit = true;
                    break;
                }
                yield return null;
            }

            if (!exit) SelectChunk();
        }
    }

    public void UpdateChunk(Chunk c)
    {
        if (ToUpdate.Contains(c)) return;

        ToUpdate.Add(c);
        UpdateChunks = true;
    }
    public void Invoke(Action action)
    {
        Actions.Add(action);
        invoke = true;
    }
    void SelectChunk()
    {
        if (!Circle.isActive && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 point = camera.position;
            point -= new Vector3Int((int) point.x, (int) point.y, (int) point.z);
            Vector3 dir = Game.camera.ScreenPointToRay(Input.mousePosition).direction;
            float add;

            for (int i = 0; i < 100; i++)
            {
                add = 1f / Mathf.Max(dir.x, dir.y, dir.z);
                dir += dir * add;
                point += dir;

                if (dir.x > 1f) dir.x -= 1f;
                if (dir.y > 1f) dir.y -= 1f;
                if (dir.z > 1f) dir.z -= 1f;

                //    dir += Vector3.one * .00001f;

                Debug.DrawLine(point, point + Vector3.up / 4f, Color.red, .5f);
                if (GetBlock(point) != null)
                {
                    tempchunk = GetChunk((int) point.x, (int) point.z);

                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        if (SelectedChunks.Contains(tempchunk)) UnselectChunk(tempchunk);
                        else SelectChunk(tempchunk);
                    }
                    else
                    {
                        while (SelectedChunks.Count > 0) UnselectChunk(SelectedChunks[0]);
                        SelectChunk(tempchunk);
                    }

                    return;
                }
            }
            while (SelectedChunks.Count > 0) UnselectChunk(SelectedChunks[0]);
        }
    }
    void SelectChunk(Chunk c)
    {
        if (c.building != null)
            foreach (Chunk cc in c.building.Chunks)
            {
                SelectedChunks.Add(cc);
                SetChunkTint(cc, Color.green);
            }
        else
        {
            SelectedChunks.Add(c);
            SetChunkTint(c, Color.green);
        }
    }
    void UnselectChunk(Chunk c)
    {
        if (c.building != null)
            foreach (Chunk cc in c.building.Chunks)
            {
                SelectedChunks.Remove(cc);
                ResetChunkTint(cc);
            }
        else
        {
            SelectedChunks.Remove(c);
            ResetChunkTint(c);
        }
    }
    public Chunk GetChunk(int x, int z) => Chunks[x / Chunk.maxX, z / Chunk.maxZ];
    public void ClearChunksTint()
    {
        foreach (Chunk c in Chunks)
            foreach (Renderer r in c.parent.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = Game.material;
    }
    void ResetChunkTint(Chunk c)
    {
        foreach (Renderer r in c.parent.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = Game.material;
    }
    void SetChunkTint(Chunk c, Color clr)
    {
        foreach (Renderer r in c.parent.GetComponentsInChildren<Renderer>())
            r.material.color = clr;
    }
    public void StartBuilding<T>() where T : Building, new()
    {
        if (SelectedChunks.Count == 0) return;

        bool selected(int x, int z) => x < 0 || z < 0 || x > sizeX - 1 || z > sizeZ - 1 || SelectedChunks.Contains(Chunks[x, z]);

        foreach (Chunk c in SelectedChunks)
        {
            if (selected(c.X + 1, c.Z) ||
                selected(c.X - 1, c.Z) ||
                selected(c.X, c.Z + 1) ||
                selected(c.X, c.Z - 1)) continue;

            return;
        }

        Building building;
        if (SelectedChunks[0].building != null) building = SelectedChunks[0].building;
        else
        {
            building = new T();
            Game.Buildings.Add(building);
        }

        builder.building = building;
        builder.OldCameraPosition = Game.camera.transform.position;
        builder.OldCameraRotation = Game.camera.transform.rotation;

        Chunk ch = SelectedChunks[0];
        Game.camera.transform.position = new Vector3(ch.Blocks[0].GetLength(0), ch.Blocks.Count + 5, ch.Blocks[0].GetLength(1));

        foreach (Chunk c in Chunks) SetChunkTint(c, Color.gray);
        foreach (Chunk c in SelectedChunks)
        {
            SetChunkTint(c, Color.white);
            c.building = building;
        }
        building.Chunks = SelectedChunks.ToArray();
        SelectedChunks.Clear();

        Game.Building = true;
        builder.enabled = true;
    }

    public bool SetBlock(int x, int y, int z, Block b, bool updatefast = false, bool update = true)
    {
        if (x < 0 || x > sizeX * Chunk.maxX - 1 || z < 0 || z > sizeZ * Chunk.maxZ - 1 || Chunks[x / Chunk.maxX, z / Chunk.maxZ] == null) return false;

        return Chunks[x / Chunk.maxX, z / Chunk.maxZ].SetBlock(x % Chunk.maxX, y, z % Chunk.maxZ, b, updateFast : updatefast, update : update);
    }
    public void RemoveBlock(int x, int y, int z, bool shootEvent = true)
    {
        if (x < 0 || x > sizeX * Chunk.maxX - 1 || z < 0 || z > sizeZ * Chunk.maxZ - 1 || Chunks[x / Chunk.maxX, z / Chunk.maxZ] == null) return;

        Chunks[x / Chunk.maxX, z / Chunk.maxZ].RemoveBlock(x % Chunk.maxX, y, z % Chunk.maxZ, shootEvent);
    }
    public Block GetBlock(int x, int y, int z)
    {
        if (x < 0 || x > sizeX * Chunk.maxX - 1 || z < 0 || z > sizeZ * Chunk.maxZ - 1 || Chunks[x / Chunk.maxX, z / Chunk.maxZ] == null) return null;

        return Chunks[x / Chunk.maxX, z / Chunk.maxZ].GetBlock(x % Chunk.maxX, y, z % Chunk.maxZ);
    }
    public Block GetBlock(Vector3 pos) => GetBlock((int) pos.x, (int) pos.y, (int) pos.z);
}