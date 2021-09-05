using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCopy
{
    public int X, Z;
    public BlockList Blocks;

    public ChunkCopy(int x, int z, BlockList blocks)
    {
        X = x;
        Z = z;
        Blocks = blocks;
    }
}