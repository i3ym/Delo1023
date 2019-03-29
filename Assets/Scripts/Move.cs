using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public const string Horizontal = "Horizontal";
    public const string Vertical = "Vertical";
    public const string Diagonal = "Diagonal";
    public const string MouseX = "Mouse X";
    public const string MouseY = "Mouse Y";

    new Transform transform;
    static Vector3 up = Vector3.up;
    float speed = 1f;
    float rotX = 0;

    void Start()
    {
        transform = Camera.main.transform;
        StartCoroutine(MouseMoveInWorld());
    }
    void Update()
    {
        if (!Game.Building) return;

        if (Input.GetKeyDown(KeyCode.LeftShift)) speed = 1.5f;
        else if (Input.GetKeyUp(KeyCode.LeftShift)) speed = 1f;

        rotX -= Input.GetAxis(MouseY) * 4;

        if (rotX > 89) rotX = 89;
        else if (rotX < -89) rotX = -89;

        transform.rotation = Quaternion.Euler(rotX, transform.eulerAngles.y + Input.GetAxis(MouseX) * 4, 0);
    }
    void FixedUpdate()
    {
        const float hitbox = .4f;
        Vector3 add;

        if (Game.Building) add = (transform.forward * (Input.GetAxis(Vertical) * speed) + transform.right * (Input.GetAxis(Horizontal) * speed) + transform.up * (Input.GetAxis(Diagonal) * speed)) / 2f;
        else add = (new Vector3(Mathf.Sin(transform.eulerAngles.y * Mathf.Deg2Rad), 0, Mathf.Cos(transform.eulerAngles.y * Mathf.Deg2Rad)) * Input.GetAxis(Vertical) +
            transform.right * Input.GetAxis(Horizontal) + up * Input.GetAxis(Diagonal)) / 2f;

        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;

        if (World.GetBlock(x + hitbox, y, z) != null && add.x > 0f) add.x = 0f;
        else if (World.GetBlock(x - hitbox, y, z) != null && add.x < 0f) add.x = 0f;
        if (World.GetBlock(x, y + hitbox, z) != null && add.y > 0f) add.y = 0f;
        else if (World.GetBlock(x, y - hitbox, z) != null && add.y < 0f) add.y = 0f;
        if (World.GetBlock(x, y, z + hitbox) != null && add.z > 0f) add.z = 0f;
        else if (World.GetBlock(x, y, z - hitbox) != null && add.z < 0f) add.z = 0f;

        transform.position += add;
    }

    IEnumerator MouseMoveInWorld()
    {
        Vector3 startPos;
        bool move;
        float time;
        WaitUntil wait = new WaitUntil(() => Input.GetMouseButtonDown(0));

        while (true)
        {
            yield return wait;

            move = false;
            time = Time.time;

            startPos = Input.mousePosition;
            while (Input.GetMouseButton(0))
            {
                if ((Input.mousePosition - startPos).magnitude > 10f)
                {
                    move = true;
                    break;
                }
                yield return null;
            }

            if (!move) continue;

            while (Input.GetMouseButton(0))
            {
                rotX -= Input.GetAxis(MouseY) * 4;

                if (rotX < 0) rotX = 0;
                else if (rotX > 89) rotX = 89;

                transform.rotation = Quaternion.Euler(rotX, transform.eulerAngles.y + Input.GetAxis(MouseX) * 4, 0);
                yield return null;
            }
        }
    }
}