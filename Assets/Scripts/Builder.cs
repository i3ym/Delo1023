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
    World world = null;
    [SerializeField]
    BlockChooser blockChooser = null;

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
    void OnPostRender() => SetOutline();

    void SetOutline()
    {
        if (Physics.Raycast(camera.position, camera.forward, out hit, 500f, layerMask))
        {
            const float size = 1.05f;
            const float minadd = (size - 1f) / 2f;
            Color clr = new Color(.1f, .1f, .1f);
            Vector3 blockPos = new Vector3((int) (hit.point.x - hit.normal.x * .01f) - minadd, (int) (hit.point.y - hit.normal.y * .01f) - minadd, (int) (hit.point.z - hit.normal.z * .01f) - minadd);
            Vector3 temp;

            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(clr);

            temp = new Vector3();
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
        if (move.enabled && Physics.Raycast(camera.position, camera.forward, out hit, 500f, layerMask))
        {
            int x = (int) (hit.point.x - hit.normal.x * .01f) + (int) hit.normal.x;
            int y = (int) (hit.point.y - hit.normal.y * .01f) + (int) hit.normal.y;
            int z = (int) (hit.point.z - hit.normal.z * .01f) + (int) hit.normal.z;

            foreach (Chunk c in building.Chunks)
                if (c == world.GetChunk(x, z))
                {
                    if (Game.Money - Block.Blocks[selectedBlock].Price >= 0)
                    {
                        if (world.SetBlock(x, y, z, Block.Blocks[selectedBlock].Instance(), true))
                            Game.Money -= Block.Blocks[selectedBlock].Price;
                    }
                    return;
                }
        }
    }
    void RemoveBlock()
    {
        if (Physics.Raycast(camera.position, camera.forward, out hit, 500f, layerMask))
        {
            MultiblockPartComponent mpc = world.GetBlock((int) (hit.point.x - hit.normal.x * .01f), (int) (hit.point.y - hit.normal.y * .01f), (int) (hit.point.z - hit.normal.z * .01f)).GetComponent<MultiblockPartComponent>();
            if (mpc != null) Game.Money += mpc.parentBlock.Price;
            else Game.Money += Block.Blocks[selectedBlock].Price;
            world.RemoveBlock((int) (hit.point.x - hit.normal.x * .01f), (int) (hit.point.y - hit.normal.y * .01f), (int) (hit.point.z - hit.normal.z * .01f));
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