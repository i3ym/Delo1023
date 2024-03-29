﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class World : MonoBehaviour
{
    public static int sizeX = 10;
    public static int sizeZ = 10;

    public static Builder builder;
    static Chunk[, ] Chunks = new Chunk[sizeX, sizeZ];
    public static List<Chunk> SelectedChunks = new List<Chunk>();
    static RaycastHit hit;
    new static Transform camera;
    static System.Random rnd = new System.Random();
    static bool invoke = false;
    static List<Chunk> ToUpdate = new List<Chunk>();
    static bool UpdateChunks = false;

    static Chunk tempchunk;

    void Start()
    {
        Game.world = this;
        camera = Game.camera.transform;

        builder = gameObject.GetComponent<Builder>();

        for (int x = 0; x < sizeX; x++)
            for (int z = 0; z < sizeZ; z++)
                Chunks[x, z] = new Chunk(x, z);

        StartCoroutine(AddVillagersCoroutine());
        StartCoroutine(SelectChunkCoroutine());
    }
    void LateUpdate()
    {
        if (UpdateChunks)
        {
            UpdateChunks = false;
            foreach (Chunk c in ToUpdate)
                if (c != null) MeshCreator.UpdateMesh(c, c.Blocks);
            ToUpdate.Clear();
        }
    }

    IEnumerator AddVillagersCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(rnd.Next(3, 18 - Math.Min(100, Game.Level) * 15)); //TODO dont forget to 300 and 1800

            if (Game.Villagers < Game.VillagersMax) Game.Villagers++;
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

    public static void UpdateChunk(Chunk c)
    {
        if (ToUpdate.Contains(c)) return;

        ToUpdate.Add(c);
        UpdateChunks = true;
    }
    static void SelectChunk()
    {
        if (!Circle.isActive && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3Int? pos = BlockRaycast.RaycastBlockPosition(camera.position, Game.camera.ScreenPointToRay(Input.mousePosition).direction, .1f, 1000);
            if (!pos.HasValue)
            {
                while (SelectedChunks.Count > 0) UnselectChunk(SelectedChunks[0]);
                return;
            }

            tempchunk = GetChunkByBlock(pos.Value.x, pos.Value.z);

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
        }
    }
    static void SelectChunk(Chunk c)
    {
        if (c.building != null)
            foreach (Chunk cc in c.building.Chunks)
            {
                SelectedChunks.Add(cc);
                SetChunkMaterial(cc, Game.materialSelected);
            }
        else
        {
            SelectedChunks.Add(c);
            SetChunkMaterial(c, Game.materialSelected);
        }
    }
    static void UnselectChunk(Chunk c)
    {
        if (c.building != null)
            foreach (Chunk cc in c.building.Chunks)
            {
                SelectedChunks.Remove(cc);
                SetChunkMaterial(cc, Game.material);
            }
        else
        {
            SelectedChunks.Remove(c);
            SetChunkMaterial(c, Game.material);
        }
    }
    public static Chunk GetChunkByBlock(int x, int z) => Chunks[x / Chunk.maxX, z / Chunk.maxZ];
    public static void ClearChunksTint()
    {
        foreach (Chunk c in Chunks)
            foreach (Renderer r in c.parent.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = Game.material;
    }
    static void SetChunkMaterial(Chunk c, Material mat)
    {
        foreach (Renderer r in c.parent.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = mat;
    }
    public static void StartBuilding<T>() where T : Building, new()
    {
        if (SelectedChunks.Count == 0) return;

        bool selected(int x, int z) => x > -1 && z > -1 && x < sizeX && z < sizeZ && SelectedChunks.Contains(Chunks[x, z]);

        Chunk ch;
        for (int i = 0; i < SelectedChunks.Count - 1; i++)
        {
            ch = SelectedChunks[i];
            if (selected(ch.X + 1, ch.Z) ||
                selected(ch.X - 1, ch.Z) ||
                selected(ch.X, ch.Z + 1) ||
                selected(ch.X, ch.Z - 1)) continue;

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

        foreach (Chunk c in Chunks) SetChunkMaterial(c, Game.materialUnselected);
        foreach (Chunk c in SelectedChunks)
        {
            SetChunkMaterial(c, Game.material);
            c.building = building;
        }
        building.Chunks.Clear();
        building.Chunks.AddRange(SelectedChunks);
        SelectedChunks.Clear();

        Game.Building = true;
        builder.enabled = true;
    }

    public static bool SetBlock(int x, int y, int z, Block b, bool takeMoney = true)
    {
        if (x < 0 || x > sizeX * Chunk.maxX - 1 || z < 0 || z > sizeZ * Chunk.maxZ - 1 || Chunks[x / Chunk.maxX, z / Chunk.maxZ] == null) return false;

        return Chunks[x / Chunk.maxX, z / Chunk.maxZ].SetBlock(x % Chunk.maxX, y, z % Chunk.maxZ, b, takeMoney : takeMoney);
    }
    public static bool SetBlock(Vector3 pos, Block b, bool takeMoney = true) => SetBlock((int) pos.x, (int) pos.y, (int) pos.z, b, takeMoney);
    public static bool SetBlock(Vector3Int pos, Block b, bool takeMoney = true) => SetBlock(pos.x, pos.y, pos.z, b, takeMoney);
    public static void RemoveBlock(int x, int y, int z, bool shootEvent = true, bool takeMoney = true)
    {
        if (x < 0 || x > sizeX * Chunk.maxX - 1 || z < 0 || z > sizeZ * Chunk.maxZ - 1 || Chunks[x / Chunk.maxX, z / Chunk.maxZ] == null) return;

        Chunks[x / Chunk.maxX, z / Chunk.maxZ].RemoveBlock(x % Chunk.maxX, y, z % Chunk.maxZ, shootEvent, takeMoney);
    }
    public static void RemoveBlock(Vector3 pos, bool shootEvent = true, bool takeMoney = true) => RemoveBlock((int) pos.x, (int) pos.y, (int) pos.z, shootEvent, takeMoney);
    public static void RemoveBlock(Vector3Int pos, bool shootEvent = true, bool takeMoney = true) => RemoveBlock(pos.x, pos.y, pos.z, shootEvent, takeMoney);
    public static Block GetBlock(int x, int y, int z)
    {
        if (x < 0 || x > sizeX * Chunk.maxX - 1 || z < 0 || z > sizeZ * Chunk.maxZ - 1 || Chunks[x / Chunk.maxX, z / Chunk.maxZ] == null) return null;

        return Chunks[x / Chunk.maxX, z / Chunk.maxZ].GetBlock(x % Chunk.maxX, y, z % Chunk.maxZ);
    }
    public static Block GetBlock(float x, float y, float z) => GetBlock((int) x, (int) y, (int) z);
    public static Block GetBlock(Vector3 pos) => GetBlock((int) pos.x, (int) pos.y, (int) pos.z);
    public static Block GetBlock(Vector3Int pos) => GetBlock(pos.x, pos.y, pos.z);
}