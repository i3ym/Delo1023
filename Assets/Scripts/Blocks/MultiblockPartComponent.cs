using UnityEngine;

public class MultiblockPartComponent : BComponent
{
    public int X, Y, Z;

    public MultiblockPartComponent(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override bool OnBreak(int x, int y, int z)
    {
        Game.world.RemoveBlock(X, Y, Z);
        return true;
    }
}