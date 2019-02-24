using UnityEngine;

public abstract class Building
{
    public abstract byte Weight { get; }
    public Chunk[] Chunks;
    int Exp = 0;
    static Block tempblock;
    static int tempExp;

    public abstract void Update(); //TODO do update per second

    public void Recalculate()
    {
        if (Weight != 0)
        {
            tempExp = Game.Exp;
            tempExp -= Exp;
            Exp = 0;

            foreach (Chunk c in Chunks)
                for (int x = 0; x < Chunk.maxX; x++)
                    for (int y = 0; y < c.sizeY; y++)
                        for (int z = 0; z < Chunk.maxZ; z++)
                        {
                            tempblock = c.GetBlock(x, y, z);
                            if (tempblock != null) Exp += tempblock.Info.Price * Weight;
                        }

            tempExp += Exp;
        }
        Game.Exp = tempExp;

        Recalc();
    }
    protected abstract void Recalc();
}