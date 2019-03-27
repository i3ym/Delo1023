using UnityEngine;

public class MultiblockComponent : BComponent
{
    public Vector3Int[] Locations { get; private set; }
    BlockInfo parentBlock;

    public MultiblockComponent(Vector3Int[] locations, BlockInfo parentBlock)
    {
        Locations = new Vector3Int[locations.Length];
        locations.CopyTo(Locations, 0);
        this.parentBlock = parentBlock;
    }

    public override bool OnPlace(int x, int y, int z, int rot)
    {
        Block b = Block.Transparent.Instance();
        b.AddComponent<MultiblockPartComponent>(x, y, z, parentBlock);

        bool canPlaceThis;
        Chunk chunk = null;

        foreach (Vector3Int pos in Locations)
        {
            if (pos.x + x < 0 || pos.x + x > World.sizeX * Chunk.maxX - 1 || pos.z + z < 0 || pos.z + z > World.sizeZ * Chunk.maxZ - 1 || World.GetBlock(x + pos.x, y + pos.y, z + pos.z) != null) return false;

            canPlaceThis = false;
            chunk = World.GetChunk(x + pos.x, z + pos.z);

            foreach (Chunk c in World.builder.building.Chunks)
                if (c == chunk)
                {
                    canPlaceThis = true;
                    break;
                }

            if (!canPlaceThis) return false;
        }

        foreach (Vector3Int pos in Locations)
            World.SetBlock(x + pos.x, y + pos.y, z + pos.z, b, update : false);

        MeshCreator.UpdateMesh(chunk, chunk.Blocks);

        return true;
    }
    public override bool OnBreak(int x, int y, int z)
    {
        foreach (Vector3Int pos in Locations)
            World.RemoveBlock(x + pos.x, y + pos.y, z + pos.z, false);

        return true;
    }
}