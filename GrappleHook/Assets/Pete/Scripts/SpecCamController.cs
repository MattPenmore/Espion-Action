using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecCamController : MonoBehaviour
{
    [SerializeField] Transform specCam;

    private Vector3 moveDir;
    private Rigidbody rb;
    private float yRotation = 45f;
    private float xRotation = 30f;

    private float camSpeed = 10f;
    private float shiftMultiplier = 3f;
    private float mouseSensitivity = 200f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    { 
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        CalculateMovement();

        CalculateRotation();
    }

    private void CalculateRotation()
    {        
        // Grab values from mouse input.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Translate mouse movement into rotation values.
        yRotation += mouseX;
        xRotation -= mouseY;
        // Clamp tilt to prevent going upside-down.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply yaw and tilt to the camera rig.
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        //specCam.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    private void CalculateMovement()
    {
        moveDir = new Vector3();

        // Forwards.
        if (Input.GetKey(KeyCode.W))
            moveDir += transform.forward;

        // Backwards.
        if (Input.GetKey(KeyCode.S))
            moveDir -= transform.forward;

        // Left.
        if (Input.GetKey(KeyCode.A))
            moveDir -= transform.right;

        // Right.
        if (Input.GetKey(KeyCode.D))
            moveDir += transform.right;

        // Up.
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E))
            moveDir.y += 1;

        // Down.
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q))
            moveDir.y -= 1;

        rb.velocity = camSpeed * moveDir.normalized * ((Input.GetKey(KeyCode.LeftShift)) ? shiftMultiplier : 1);
    }
}
