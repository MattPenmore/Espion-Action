using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkedLook : MonoBehaviour
{

    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0;

    bool isStarting = true;

    // Start is called before the first frame update
    void Start()
    {
        //startRotation = playerBody.rotation;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Break out of update loop if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return;

        // Break out for the first 1s.
        if (Time.time < 1f)
            return;

        //Look around with mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Prevent rotation from normal position when game is first loading
        if (mouseY != 0 && isStarting)
        {
            mouseY = 0;
            mouseX = 0;
            isStarting = false;
        }

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
