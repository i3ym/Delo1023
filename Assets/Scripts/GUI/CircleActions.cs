using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleActions : Circle
{
    override protected void AddItems()
    {
        items.Add(new CircleItem(sprites[0], () =>
        {
            Debug.Log("copy");
        }));
        items.Add(new CircleItem(sprites[1], () =>
        {
            Debug.Log("paste");
        }));
        items.Add(new CircleItem(sprites[2], () => MessageBox.Show("Перегенерировать чанк?", () => World.SelectedChunks.ForEach(x => x.Generate()), () => { })));
    }
}