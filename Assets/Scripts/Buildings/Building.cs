using UnityEngine;

public abstract class Building
{
    public Chunk[] Chunks;

    public abstract void Update(); //TODO do update per second

    public abstract void Recalculate();

    public static void sas()
    {

    }
}