using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Builder : MonoBehaviour
{
    public const string MouseScroll = "Mouse ScrollWheel";

    [SerializeField]
    TextMeshProUGUI TextSelectedBlock = null;
    [SerializeField]
    GameObject QuitMenu = null;
    [SerializeField]
    Button ButtonContinue = null, ButtonExit = null;
    [SerializeField]
    BlockChooser blockChooser = null;

    public Building building;
    Move move;
    int layerMask;
    int selectedBlock = 0;

    float tempScroll;

    void Start()
    {
        layerMask = LayerMask.GetMask("ChunkBuilding");
        move = GetComponent<Move>();

        ButtonContinue.onClick.AddListener(new UnityAction(() =>
        {
            move.enabled = true;
            QuitMenu.SetActive(false);
        }));
        ButtonExit.onClick.AddListener(new UnityAction(() =>
        {
            World.ClearChunksTint();

            building.Recalculate();

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
    void OnPostRender() => SetOutline();

    void SetOutline()
    {
        Vector3Int? pos = BlockRaycast.RaycastBlockPosition(Game.cameratr.position, Game.cameratr.forward, .1f, 1000);
        if (pos.HasValue && building.Chunks.Contains(World.GetChunkByBlock(pos.Value.x, pos.Value.z)))
        {
            const float size = 1.05f;
            Vector3 blockPos = pos.Value - Vector3.one * (size - 1f) / 2f;

            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(new Color(.1f, .1f, .1f));

            Vector3 temp = new Vector3();
            GL.Vertex(blockPos + temp);
            temp.y = size;
            GL.Vertex(blockPos + temp);
            GL.Vertex(blockPos + temp);
            temp.x = size;
            GL.Vertex(blockPos + temp);
            GL.Vertex(blockPos + temp);
            temp.y = 0f;
            GL.Vertex(blockPos + temp);
            GL.Vertex(blockPos + temp);
            temp.x = 0f;
            GL.Vertex(blockPos + temp);
            GL.Vertex(blockPos + temp);
            temp.z = size;
            GL.Vertex(blockPos + temp);
            GL.Vertex(blockPos + temp);
            temp.y = size;
            GL.Vertex(blockPos + temp);
            GL.Vertex(blockPos + temp);
            temp.z = 0f;
            GL.Vertex(blockPos + temp);

            temp.Set(size, 0f, 0f);
            GL.Vertex(blockPos + temp);
            temp.z = size;
            GL.Vertex(blockPos + temp);
            GL.Vertex(blockPos + temp);
            temp.y = size;
            GL.Vertex(blockPos + temp);
            GL.Vertex(blockPos + temp);
            temp.z = 0f;
            GL.Vertex(blockPos + temp);

            temp.Set(0f, 0f, size);
            GL.Vertex(blockPos + temp);
            temp.x = size;
            GL.Vertex(blockPos + temp);

            temp.Set(0f, size, size);
            GL.Vertex(blockPos + temp);
            temp.x = size;
            GL.Vertex(blockPos + temp);

            GL.End();
            GL.PopMatrix();
        }
    }
    void ChooseBlock()
    {
        tempScroll = Input.GetAxisRaw(MouseScroll);

        if (tempScroll > 0 || Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (++selectedBlock >= Block.Blocks.Count) selectedBlock = 0;
            blockChooser.ChangeSelected(selectedBlock);

            TextSelectedBlock.text = "Selected: " + selectedBlock;
        }
        else if (tempScroll < 0 || Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (--selectedBlock < 0) selectedBlock = Block.Blocks.Count - 1;
            blockChooser.ChangeSelected(selectedBlock);

            TextSelectedBlock.text = "Selected: " + selectedBlock;
        }
    }
    void PlaceBlock()
    {
        Vector3Int? pos = BlockRaycast.RaycastBlockForPlace(Game.cameratr.position, Game.cameratr.forward, .05f, 2000);
        if (move.enabled && pos.HasValue && building.Chunks.Contains(World.GetChunkByBlock(pos.Value.x, pos.Value.z)) && Game.Money - Block.Blocks[selectedBlock].Price >= 0)
            if (World.SetBlock(pos.Value, Block.Blocks[selectedBlock].Instance()))
                Game.Money -= Block.Blocks[selectedBlock].Price;
    }
    void RemoveBlock()
    {
        Vector3Int? pos = BlockRaycast.RaycastBlockPosition(Game.cameratr.position, Game.cameratr.forward, .1f, 1000);
        if (move.enabled && pos.HasValue)
        {
            MultiblockPartComponent mpc = World.GetBlock(pos.Value + (Vector3) pos.Value * float.Epsilon).GetComponent<MultiblockPartComponent>();
            if (mpc != null) Game.Money += mpc.parentBlock.Price;
            else Game.Money += Block.Blocks[selectedBlock].Price;
            World.RemoveBlock(pos.Value);
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