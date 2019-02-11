using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block
{
    public static readonly BlockInfo Transparent;
    public static readonly BlockInfo Grass;
    public static readonly BlockInfo Dirt;
    public static readonly BlockInfo Bricks;
    public static readonly BlockInfo Door;
    public static readonly BlockInfo Trumpet;
    public static readonly BlockInfo Bed;

    public BlockInfo Info;
    List<BComponent> components = new List<BComponent>();
    public byte Rotation = 0;

    static Block()
    {
        Transparent = new BlockInfoMesh("transparent", 0);
        Builder.Blocks.Remove(Transparent);

        Grass = new BlockInfo("grass", new string[] { "dirt", "dirt", "grass", "dirt", "dirt", "dirt" });
        Dirt = new BlockInfo("dirt");
        Bricks = new BlockInfo("bricks");
        Door = new BlockInfoMesh("door");
        Trumpet = new BlockInfoMesh("trumpet");
        Bed = new BlockInfoMesh("bed");
    }

    public T GetComponent<T>() where T : BComponent => components.OfType<T>().FirstOrDefault(); //TODO fix performance ?
    public T AddComponent<T>(params object[] args) where T : BComponent
    {
        T t = (T) Activator.CreateInstance(typeof(T), args);
        components.Add(t);
        return t;
    }
    public T AddComponent<T>(T t) where T : BComponent
    {
        components.Add(t);
        return t;
    }
    public BComponent AddComponent(Type type, params object[] args)
    {
        BComponent t = (BComponent) Activator.CreateInstance(type, args);
        components.Add(t);
        return t;
    }

    public bool OnPlace(int x, int y, int z)
    {
        foreach (BComponent c in components)
            if (!c.OnPlace(x, y, z)) return false;

        return true;
    }
    public bool OnBreak(int x, int y, int z)
    {
        foreach (BComponent c in components)
            if (!c.OnBreak(x, y, z)) return false;

        return true;
    }
}