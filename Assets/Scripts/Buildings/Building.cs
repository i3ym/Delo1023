using System.Collections.Generic;
using UnityEngine;

public abstract class Building
{
    public abstract byte Weight { get; }
    public List<Chunk> Chunks = new List<Chunk>();
    int Exp = 0;
    static Block tempblock;
    static long tempExp;

    public abstract void Update(); //TODO do update per second

    public void Recalculate()
    {
        if (Weight != 0)
        {
            Game.Exp -= Exp;
            Exp = 0;

            foreach (Chunk c in Chunks)
                foreach (Block b in c.Blocks)
                    if (b != null) Exp += tempblock.Info.Price * Weight;

            tempExp += Exp;
        }
        Game.Exp = tempExp;

        Recalc();
    }
    protected abstract void Recalc();
}