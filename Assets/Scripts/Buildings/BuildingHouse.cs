using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHouse : Building
{
    public override byte Weight { get; } = 4;
    int Villagers = 0;

    public override void Update() { }

    protected override void Recalc()
    {
        Game.Villagers -= Villagers;
        Villagers = 0;

        foreach (Chunk c in Chunks)
        {
            for (int x = 0; x < Chunk.maxX; x++)
                for (int y = 0; y < c.Blocks.Count; y++)
                    for (int z = 0; z < Chunk.maxZ; z++)
                        if (c.GetBlock(x, y, z)?.Info == Block.Bed) Villagers++;
        }
        Game.Villagers += Villagers;
    }
}