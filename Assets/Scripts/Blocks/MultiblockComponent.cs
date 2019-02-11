using UnityEngine;

public class MultiblockComponent : BComponent
{
    public Vector3Int[] Locations { get; private set; }

    public MultiblockComponent(params Vector3Int[] locations)
    {
        Locations = new Vector3Int[locations.Length];
        locations.CopyTo(Locations, 0);
    }

    public override bool OnPlace(int x, int y, int z)
    {
        Block b = Block.Transparent.Instance();
        b.AddComponent<MultiblockPartComponent>(x, y, z);

        bool canPlaceThis;
        Chunk chunk;

        foreach (Vector3Int pos in Locations)
        {
            if (pos.x + x < 0 || pos.x + x > World.sizeX * Chunk.maxX - 1 || pos.z + z < 0 || pos.z + z > World.sizeZ * Chunk.maxZ - 1 || Game.world.GetBlock(x + pos.x, y + pos.y, z + pos.z) != null) return false;

            canPlaceThis = false;
            chunk = Game.world.GetChunk(x + pos.x, z + pos.z);

            foreach (Chunk c in Game.world.builder.building.Chunks)
                if (c == chunk)
                {
                    canPlaceThis = true;
                    break;
                }

            if (!canPlaceThis) return false;
        }

        foreach (Vector3Int pos in Locations)
            Game.world.SetBlock(x + pos.x, y + pos.y, z + pos.z, b);

        return true;
    }
    public override bool OnBreak(int x, int y, int z)
    {
        foreach (Vector3Int pos in Locations)
            Game.world.RemoveBlock(x + pos.x, y + pos.y, z + pos.z, false);

        return true;
    }
}