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
    public static int sizeX = 5;
    public static int sizeZ = 5;

    public Builder builder;
    Chunk[, ] Chunks = new Chunk[sizeX, sizeZ];
    List<Chunk> SelectedChunks = new List<Chunk>();
    RaycastHit hit;
    int layerMask;
    new Transform camera;

    Chunk tempchunk;

    void Start()
    {
        Game.world = this;
        camera = Game.camera.transform;

        builder = gameObject.GetComponent<Builder>();
        layerMask = LayerMask.GetMask("Chunk");
        Game.buildingChooser.SetActive(false);

        for (int x = 0; x < sizeX; x++)
            for (int z = 0; z < sizeZ; z++)
                Chunks[x, z] = new Chunk(x, z, this);
    }
    void Update()
    {
        if (Game.Building) return;

        SelectChunk();
    }

    void SelectChunk()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Physics.Raycast(Game.camera.ScreenPointToRay(Input.mousePosition), out hit, 1000f, layerMask))
            { //TODO shift=
                tempchunk = Chunks[(int) hit.transform.position.x / Chunk.maxX, (int) hit.transform.position.z / Chunk.maxZ];

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

                Game.buildingChooser.SetActive(SelectedChunks.Count != 0);
            }
            else
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
                SetChunkTint(cc, Color.white);
            }
        else
        {
            SelectedChunks.Remove(c);
            SetChunkTint(c, Color.white);
        }
    }
    public Chunk GetChunk(int x, int z) => Chunks[x / Chunk.maxX, z / Chunk.maxZ];
    public void ClearChunksTint()
    {
        foreach (Chunk c in Chunks)
            foreach (Renderer r in c.parent.GetComponentsInChildren<Renderer>())
                r.material.color = Color.white;
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

    public bool SetBlock(int x, int y, int z, Block b)
    {
        if (x < 0 || x > sizeX * Chunk.maxX - 1 || z < 0 || z > sizeZ * Chunk.maxZ - 1 || Chunks[x / Chunk.maxX, z / Chunk.maxZ] == null) return false;

        return Chunks[x / Chunk.maxX, z / Chunk.maxZ].SetBlock(x % Chunk.maxX, y, z % Chunk.maxZ, b);
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
}