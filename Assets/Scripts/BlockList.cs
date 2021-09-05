using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BlockList : IEnumerable<Block>
{
    public int SizeY { get => Blocks.Count; }
    List<Block[, ]> Blocks = new List<Block[, ]>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlock(int x, int y, int z, Block b)
    {
        lock(Blocks)
        while (y >= Blocks.Count) Blocks.Add(new Block[Chunk.maxX, Chunk.maxZ]);

        Blocks[y][x, z] = b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBlock(int x, int y, int z)
    {
        if (y < Blocks.Count) Blocks[y][x, z] = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Block GetBlock(int x, int y, int z)
    {
        lock(Blocks)
        return y >= Blocks.Count ? null : Blocks[y][x, z];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Blocks.Clear();

    public BlockList Clone()
    {
        BlockList list = new BlockList();
        list.Blocks.AddRange(Blocks);
        return list;
    }

    public IEnumerator<Block> GetEnumerator()
    {
        foreach (var ba in Blocks)
            foreach (var block in ba)
                yield return block;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}