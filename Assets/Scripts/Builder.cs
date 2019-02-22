﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Builder : MonoBehaviour
{
    public const string MouseScroll = "Mouse ScrollWheel";

    public static List<BlockInfo> Blocks = new List<BlockInfo>();

    [SerializeField]
    TextMeshProUGUI TextSelectedBlock;
    [SerializeField]
    GameObject QuitMenu;
    [SerializeField]
    Button ButtonContinue, ButtonExit;
    [SerializeField]
    World world;

    [HideInInspector]
    public Vector3 OldCameraPosition;
    [HideInInspector]
    public Quaternion OldCameraRotation;
    public Building building;
    Move move;
    new Transform camera;
    int layerMask;
    RaycastHit hit;
    int selectedBlock = 0;

    float tempScroll;

    void Start()
    {
        layerMask = LayerMask.GetMask("ChunkBuilding");
        camera = Game.camera.transform;
        move = GetComponent<Move>();

        ButtonContinue.onClick.AddListener(new UnityAction(() =>
        {
            move.enabled = true;
            QuitMenu.SetActive(false);
        }));
        ButtonExit.onClick.AddListener(new UnityAction(() =>
        {
            world.ClearChunksTint();

            building.Recalculate();

            camera.position = OldCameraPosition;
            camera.rotation = OldCameraRotation;
            move.enabled = true;
            Game.Building = false;
            enabled = false;
        }));
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            move.enabled = QuitMenu.activeSelf;
            QuitMenu.SetActive(!QuitMenu.activeSelf);
        }

        ChooseBlock();
        if (Input.GetMouseButtonDown(0)) PlaceBlock();
        if (Input.GetMouseButtonDown(1)) RemoveBlock();
    }

    void ChooseBlock()
    {
        tempScroll = Input.GetAxisRaw(MouseScroll);

        if (tempScroll > 0 || Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (++selectedBlock >= Blocks.Count) selectedBlock = 0;
            TextSelectedBlock.text = "Selected: " + selectedBlock;
        }
        else if (tempScroll < 0 || Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (--selectedBlock < 0) selectedBlock = Blocks.Count - 1;
            TextSelectedBlock.text = "Selected: " + selectedBlock;
        }
    }
    void PlaceBlock()
    {
        if (move.enabled && Physics.Raycast(camera.position, camera.forward, out hit, 500f, layerMask))
        {
            int x = (int) (hit.point.x - hit.normal.x * .01f) + (int) hit.normal.x;
            int y = (int) (hit.point.y - hit.normal.y * .01f) + (int) hit.normal.y;
            int z = (int) (hit.point.z - hit.normal.z * .01f) + (int) hit.normal.z;

            foreach (Chunk c in building.Chunks)
                if (c == world.GetChunk(x, z))
                {
                    if (Game.Money - Blocks[selectedBlock].Price >= 0)
                    {
                        if (world.SetBlock(x, y, z, Blocks[selectedBlock].Instance()))
                            Game.Money -= Blocks[selectedBlock].Price;
                    }
                    break;
                }
        }
    }
    void RemoveBlock()
    {
        if (Physics.Raycast(camera.position, camera.forward, out hit, 500f, layerMask))
        {
            world.RemoveBlock((int) (hit.point.x - hit.normal.x * .01f), (int) (hit.point.y - hit.normal.y * .01f), (int) (hit.point.z - hit.normal.z * .01f));
            Game.Money += Blocks[selectedBlock].Price;
        }
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (building != null)
            foreach (Chunk ch in building.Chunks)
                foreach (Transform tr in ch.parent.transform)
                    tr.gameObject.layer = 11;

        TextSelectedBlock.gameObject.SetActive(true);
        TextSelectedBlock.text = "selected: " + selectedBlock;
    }
    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;

        foreach (Chunk ch in building.Chunks)
            if (ch.parent)
                foreach (Transform tr in ch.parent.transform)
                    if (tr) tr.gameObject.layer = 10;

        if (TextSelectedBlock) TextSelectedBlock.gameObject.SetActive(false);
        if (QuitMenu) QuitMenu.SetActive(false);
        if (move) move.enabled = true;
    }
}