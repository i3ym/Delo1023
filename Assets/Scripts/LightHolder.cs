using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightHolder
{
    public Vector3 position { get; private set; }
    public Quaternion rotation { get; private set; }
    public LightType type { get; private set; }
    public float range { get; private set; }
    public int spotAngle { get; private set; }
    public Color color { get; private set; }
    public float intensity { get; private set; }

    public LightHolder(Vector3 position, Vector3 rotation, LightType type, float range, int spotAngle, Color color, float intensity)
    {
        this.position = position;
        this.rotation = Quaternion.Euler(rotation);
        this.type = type;
        this.range = range;
        this.spotAngle = spotAngle;
        this.color = color;
        this.intensity = intensity;
    }
}