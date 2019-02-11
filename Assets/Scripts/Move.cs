using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public const string Horizontal = "Horizontal";
    public const string Vertical = "Vertical";
    public const string Diagonal = "Diagonal"; //e q
    public const string MouseX = "Mouse X";
    public const string MouseY = "Mouse Y";

    static Vector3 up = Vector3.up;
    new Rigidbody camera;
    float speed = 1f;
    float rotX = 0;

    Vector3 tempForward;

    void Start()
    {
        camera = Game.camera.GetComponent<Rigidbody>();
    }
    void Update()
    {
        if (Game.Building)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) speed = 1.5f;
            else if (Input.GetKeyUp(KeyCode.LeftShift)) speed = 1f;
        }

        if (Game.Building || Input.GetMouseButton(1))
        {
            rotX -= Input.GetAxis(MouseY) * 4;

            if (!Game.Building && rotX < 0) rotX = 0;
            else if (rotX > 89) rotX = 89;
            else if (rotX < -89) rotX = -89;

            camera.rotation = Quaternion.Euler(rotX, camera.rotation.eulerAngles.y + Input.GetAxis(MouseX) * 4, 0);
        }
    }
    void FixedUpdate() //TODO diagonal working when not needed
    {
        if (Game.Building) camera.velocity = (camera.transform.forward * (Input.GetAxis(Vertical) * speed) + camera.transform.right * (Input.GetAxis(Horizontal) * speed) + up * (Input.GetAxis(Diagonal) * speed)) * 20;
        else
        {
            if (Input.GetAxis(Vertical) != 0f)
                tempForward.Set(camera.transform.forward.x, 0, camera.transform.forward.z);

            camera.velocity = (tempForward * Input.GetAxis(Vertical) + camera.transform.right * Input.GetAxis(Horizontal) + camera.transform.up * Input.GetAxis(Diagonal)) * 20;
        }
    }
}