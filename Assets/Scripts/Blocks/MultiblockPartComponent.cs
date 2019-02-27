using UnityEngine;

public class MultiblockPartComponent : BComponent
{
    public BlockInfo parentBlock;
    public int X, Y, Z;

    public MultiblockPartComponent(int x, int y, int z, BlockInfo parentBlock)
    {
        X = x;
        Y = y;
        Z = z;
        this.parentBlock = parentBlock;
    }

    public override bool OnBreak(int x, int y, int z)
    {
        Game.world.RemoveBlock(X, Y, Z);
        return true;
    }
}