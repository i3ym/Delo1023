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
    public static readonly BlockInfo Planks;
    public static readonly BlockInfo Bricks;
    public static readonly BlockInfo Door;
    public static readonly BlockInfo Trumpet;
    public static readonly BlockInfo Bed;
    public static readonly BlockInfo DoorSupermarket;
    public static readonly BlockInfo GlassPane;
    public static readonly BlockInfo StreetLight;

    public BlockInfo Info;
    Dictionary<Type, BComponent> components = new Dictionary<Type, BComponent>();
    public byte Rotation = 0;

    static Block()
    {
        Transparent = new BlockInfoMesh("transparent", 0);
        Builder.Blocks.Remove(Transparent);

        Grass = new BlockInfo("grass", new string[] { "dirt", "dirt", "grass", "dirt", "dirt", "dirt" }, false, price : 0);
        Dirt = new BlockInfo("dirt", price : 0);
        Planks = new BlockInfo("planks", price : 10);
        Bricks = new BlockInfo("bricks", price : 10);
        Door = new BlockInfoMesh("door");
        Trumpet = new BlockInfoMesh("trumpet");
        Bed = new BlockInfoMesh("bed", price : 20);
        GlassPane = new BlockInfoMesh("glass_pane", price : 5, components : new Dictionary<Type, object[]>()
        {
            {
                typeof(MultimodelComponent),
                new object[] { "glass_pane_corner", "glass_pane_side", "glass_pane_center" }
            }
        });
        StreetLight = new BlockInfoMesh("street_light", components : new Dictionary<Type, object[]>()
        {
            {
                typeof(LightComponent),
                new object[] { new LightHolder(new Vector3(0f, 3.94141f, -.245556f), new Vector3(70f, 180f, 0f), LightType.Spot, 10f, 80, Color.white, 2f) }
            }
        });
    }

    public T GetComponent<T>() where T : BComponent => components.ContainsKey(typeof(T)) ? (T) components[typeof(T)] : null;
    public T AddComponent<T>(params object[] args) where T : BComponent
    {
        T t = (T) Activator.CreateInstance(typeof(T), args);
        components.Add(typeof(T), t);
        return t;
    }
    public T AddComponent<T>(T t) where T : BComponent
    {
        components.Add(typeof(T), t);
        return t;
    }
    public BComponent AddComponent(Type type, params object[] args)
    {
        BComponent t = (BComponent) Activator.CreateInstance(type, args);
        components.Add(type, t);
        return t;
    }

    public bool OnPlace(int x, int y, int z, int rot)
    {
        foreach (BComponent c in components.Values)
            if (!c.OnPlace(x, y, z, rot)) return false;

        return true;
    }
    public bool OnBreak(int x, int y, int z)
    {
        foreach (BComponent c in components.Values)
            if (!c.OnBreak(x, y, z)) return false;

        return true;
    }
}