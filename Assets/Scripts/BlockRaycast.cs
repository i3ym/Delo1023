using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockRaycast
{
    static(Vector3?, Vector3Int?) RaycastPosition(Vector3 point, Vector3 dir, float step, int count)
    {
        Vector3Int pointint = new Vector3Int((int) point.x, (int) point.y, (int) point.z);
        Vector3Int lastPos;

        for (int i = 0; i < count; i++)
        {
            lastPos = pointint;
            while (pointint == lastPos)
            {
                point += dir * step;
                pointint.Set((int) point.x, (int) point.y, (int) point.z);
            }

            if (World.GetBlock(point) != null) return (point, pointint);
        }
        return (null, null);
    }
    public static Vector3Int? RaycastBlockPosition(Vector3 point, Vector3 dir, float step, int count) => RaycastPosition(point, dir, step, count).Item2;
    public static Block RaycastBlock(Vector3 point, Vector3 dir, float step, int count)
    {
        Vector3Int? pos = RaycastPosition(point, dir, step, count).Item2;
        return pos.HasValue ? World.GetBlock(pos.Value) : null;
    }
    public static Vector3Int? RaycastBlockForPlace(Vector3 point, Vector3 dir, float step, int count)
    {
        var posn = RaycastPosition(point, dir, step, count);
        if (!posn.Item1.HasValue) return null;

        Vector3 pos = posn.Item1.Value - posn.Item2.Value;
        float x = Mathf.Min(1f - pos.x, pos.x);
        float y = Mathf.Min(1f - pos.y, pos.y);
        float z = Mathf.Min(pos.z, 1f - pos.z);

        if (x < y)
        {
            if (z < x) return posn.Item2.Value + new Vector3Int(0, 0, pos.z >.5f ? 1 : -1);
            else return posn.Item2.Value + new Vector3Int(pos.x >.5f ? 1 : -1, 0, 0);
        }
        else
        {
            if (z < y) return posn.Item2.Value + new Vector3Int(0, 0, pos.z >.5f ? 1 : -1);
            else return posn.Item2.Value + new Vector3Int(0, pos.y >.5f ? 1 : -1, 0);
        }
    }
}