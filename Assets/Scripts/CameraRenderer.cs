using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CameraRenderer : MonoBehaviour
{
    protected const int renderSize = 256;
    public static GameObject meshgo;
    protected static MeshFilter mf;
    protected static MeshRenderer mr;
    new static Camera camera;

    [SerializeField]
    Mesh cubeMesh = null, cubeMeshMultitexture = null;

    void Awake()
    {
        camera = GetComponent<Camera>();

        RenderTexture rt = RenderTexture.GetTemporary(renderSize, renderSize);
        RenderTexture.active = rt;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0, 0, 0, 0);
        camera.targetTexture = rt;

        meshgo = new GameObject();
        meshgo.transform.eulerAngles = new Vector3(0f, 45f, 0f);
        meshgo.layer = 15;

        mf = meshgo.AddComponent<MeshFilter>();
        mr = meshgo.AddComponent<MeshRenderer>();
        mr.material = new Material(Game.material);

        foreach (BlockInfo b in Block.Blocks)
            if (b != Block.Transparent)
                AddRender(b, b.textures);

        RenderTexture.ReleaseTemporary(RenderTexture.active);

        Destroy(meshgo);
        Destroy(gameObject);
    }

    void AddRender(BlockInfo block, string[] textures = null)
    {
        Texture2D outtexture = new Texture2D(renderSize, renderSize);

        meshgo.transform.position = gameObject.transform.position + new Vector3(0f, -1f, 1.5f);
        camera.transform.LookAt(meshgo.transform);

        if (block is BlockInfoMesh)
        {
            mf.mesh = Game.Meshes[block.Name];

            Vector3 sizef = mf.mesh.bounds.max - mf.mesh.bounds.min;
            Vector3Int meshsize = new Vector3Int(Mathf.CeilToInt(sizef.x), Mathf.CeilToInt(sizef.y), Mathf.CeilToInt(sizef.z));
            int meshmax = Math.Max(meshsize.x, meshsize.z);

            meshgo.transform.position += new Vector3(-(meshsize.z - 1) * .25f - (meshsize.x - 1) * .5f, -(meshsize.y - 1) - .5f, meshmax / 4f + meshsize.y / 1.5f - 1f);
        }
        else mf.mesh = cubeMesh;

        if (textures == null) mr.material.mainTexture = Game.textures[block.Name];
        else
        {
            mf.mesh = cubeMeshMultitexture;

            Texture2D tex = new Texture2D(Game.textures[textures[0]].width * 4, Game.textures[textures[0]].height * 4);

            for (int i = 0; i < 6; i++)
                tex.SetPixels((i % 4) * tex.width / 4, (i / 4) * tex.height / 4, tex.width / 4, tex.height / 4, Game.textures[textures[i]].GetPixels());
            tex.Apply();

            mr.material.mainTexture = tex;
        }

        camera.Render();
        outtexture.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
        outtexture.Apply();
        outtexture.SetPixel(0, 0, Color.black);

        Game.BlockRenders.Add(block.Name, outtexture);
    }
}