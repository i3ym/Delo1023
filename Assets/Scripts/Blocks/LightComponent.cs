using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightComponent : BComponent
{
    public LightHolder[] lights;
    List<GameObject> lightHolders = new List<GameObject>();

    public LightComponent(params LightHolder[] lights)
    {
        this.lights = lights;
    }

    public override bool OnPlace(int x, int y, int z, int rot)
    {
        GameObject go;
        Light l;

        foreach (LightHolder light in lights)
        {
            go = new GameObject("light");
            if (rot == 0)
            {
                go.transform.position = light.position + new Vector3(x + .5f, y, z + .5f);
                go.transform.rotation = light.rotation;
            }
            else if (rot == 1)
            {
                go.transform.position = Chunk.angle1 * light.position + new Vector3(x + .5f, y, z + .5f);
                go.transform.rotation = Chunk.angle1 * light.rotation;
            }
            else if (rot == 2)
            {
                go.transform.position = Chunk.angle2 * light.position + new Vector3(x + .5f, y, z + .5f);
                go.transform.rotation = Chunk.angle2 * light.rotation;
            }
            else if (rot == 3)
            {
                go.transform.position = Chunk.angle3 * light.position + new Vector3(x + .5f, y, z + .5f);
                go.transform.rotation = Chunk.angle3 * light.rotation;
            }

            l = go.AddComponent<Light>();
            l.type = light.type;
            l.range = light.range;
            l.spotAngle = light.spotAngle;
            l.color = light.color;
            l.intensity = light.intensity;
            l.shadows = LightShadows.Hard;
            l.renderMode = LightRenderMode.ForcePixel;
            l.bounceIntensity = 0f;

            lightHolders.Add(go);
        }

        return true;
    }
    public override bool OnBreak(int x, int y, int z)
    {
        foreach (GameObject go in lightHolders) GameObject.Destroy(go);
        lightHolders.Clear();

        return true;
    }
}