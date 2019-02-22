using UnityEngine;

public abstract class Building
{
    public abstract byte Weight { get; }
    public Chunk[] Chunks;
    int Level = 0;
    static Block tempblock;

    public abstract void Update(); //TODO do update per second

    public void Recalculate()
    {
        if (Weight != 0)
        {
            Game.Level -= Level;
            Level = 0;

            foreach (Chunk c in Chunks)
                for (int x = 0; x < Chunk.maxX; x++)
                    for (int y = 0; y < c.sizeY; y++)
                        for (int z = 0; z < Chunk.maxZ; z++)
                        {
                            tempblock = c.GetBlock(x, y, z);
                            if (tempblock != null) Level += tempblock.Info.Price * Weight;
                        }

            Game.Level += Level;
        }

        Recalc();
    }
    protected abstract void Recalc();
}