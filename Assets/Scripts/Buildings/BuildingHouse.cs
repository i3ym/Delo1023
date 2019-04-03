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
        Game.VillagersMax -= Villagers;
        Villagers = 0;

        foreach (Chunk c in Chunks)
            foreach (Block b in c.Blocks)
                if (b != null && b.Info == Block.Bed) Villagers++;

        Game.VillagersMax += Villagers;
    }
}