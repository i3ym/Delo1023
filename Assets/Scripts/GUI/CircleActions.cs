using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleActions : Circle
{
    override protected void AddItems()
    {
        items.Add(new CircleItem(sprites[0], () =>
        {
            List<ChunkCopy> copies = new List<ChunkCopy>();

            int minx = int.MaxValue;
            int minz = int.MaxValue;

            foreach (Chunk c in World.SelectedChunks)
            {
                if (minx > c.X) minx = c.X;
                if (minz > c.Z) minx = c.Z;
            }

            foreach (Chunk c in World.SelectedChunks)
                copies.Add(new ChunkCopy(c.X - minx, c.Z - minz, c.Blocks.Clone()));
        }));
        items.Add(new CircleItem(sprites[1], () =>
        {
            Debug.Log("paste");
        }));
        items.Add(new CircleItem(sprites[2], () => MessageBox.Show("Перегенерировать чанк?", () => World.SelectedChunks.ForEach(x => x.Generate()), () => { })));
    }
}