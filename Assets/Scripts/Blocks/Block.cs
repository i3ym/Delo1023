using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block
{
    public static List<BlockInfo> Blocks = new List<BlockInfo>();
    public static BlockInfo Transparent { get; private set; }
    public static BlockInfo Grass { get; private set; }
    public static BlockInfo Dirt { get; private set; }
    public static BlockInfo Planks { get; private set; }
    public static BlockInfo Log { get; private set; }
    public static BlockInfo Leaves { get; private set; }
    public static BlockInfo Bricks { get; private set; }
    public static BlockInfo Door { get; private set; }
    public static BlockInfo Trumpet { get; private set; }
    public static BlockInfo Bed { get; private set; }
    public static BlockInfo DoorSupermarket { get; private set; }
    public static BlockInfo GlassPane { get; private set; }
    public static BlockInfo StreetLight { get; private set; }
    public static BlockInfo WallLamp { get; private set; }

    public BlockInfo Info;
    Dictionary<Type, BComponent> components = new Dictionary<Type, BComponent>();
    public byte Rotation = 0;

    public static void CreateBlocks()
    {
        Transparent = new BlockInfoMesh("transparent", 0);
        Blocks.Remove(Transparent);

        Func<int, int, int, int, bool> wallLampFunc = (x, y, z, rot) =>
        {
            if (rot == 0) return World.GetBlock(x, y, z + 1) != null;
            if (rot == 1) return World.GetBlock(x + 1, y, z) != null;
            if (rot == 2) return World.GetBlock(x, y, z - 1) != null;
            if (rot == 3) return World.GetBlock(x - 1, y, z) != null;
            return true;
        };

        Grass = new BlockInfo("grass", new string[] { "dirt", "dirt", "grass", "dirt", "dirt", "dirt" }, false, price : 0);
        Dirt = new BlockInfo("dirt", price : 0);
        Planks = new BlockInfo("planks", price : 10);
        Log = new BlockInfo("log", price : 0);
        Leaves = new BlockInfo("leaves", price : 0);
        Bricks = new BlockInfo("bricks", price : 10);
        Door = new BlockInfoMesh("door");
        DoorSupermarket = new BlockInfoMesh("door_supermarket");
        Trumpet = new BlockInfoMesh("trumpet");
        Bed = new BlockInfoMesh("bed", price : 20);
        GlassPane = new BlockInfoMesh("glass_pane", price : 5, components : new Dictionary<Type, object[]>()
        {
            {
                typeof(MultimodelComponent),
                new object[] { "glass_pane_corner", "glass_pane_side", "glass_pane_center" }
            }
        });
        StreetLight = new BlockInfoMesh("street_light", price : 27, components : new Dictionary<Type, object[]>()
        {
            {
                typeof(LightComponent),
                new object[] { new LightHolder(new Vector3(0f, 3.94141f, -.245556f), new Vector3(70f, 180f, 0f), LightType.Spot, 10f, 80, Color.white, 2f) }
            }
        });
        WallLamp = new BlockInfoMesh("wall_lamp", price : 13, components : new Dictionary<Type, object[]>()
        {
            {
                typeof(PlaceRestrictorComponent),
                new object[] { wallLampFunc }
            },
            {
                typeof(LightComponent),
                new object[] { new LightHolder(new Vector3(0f, .5f, .4f), new Vector3(0f, 0f, 0f), LightType.Point, 20f, 0, Color.white, 1f) }
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