using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;

    public float speed;

    bool isPlayerGrounded;

    public Vector3 move;

    GrapplingHook hook;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        hook = GetComponent<GrapplingHook>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfGrounded();

        if(isPlayerGrounded)
        {
            rb.velocity = Vector3.zero;
        }

        if(!hook.hasHooked || isPlayerGrounded)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * Time.deltaTime;

            rb.MovePosition(transform.position + move);
        }
    }

    void CheckIfGrounded()
    {
        RaycastHit hit;
        float dist = 1.1f;
        Vector3 dir = Vector3.down;

        if (Physics.Raycast(transform.position, dir, out hit, dist))
        {
            isPlayerGrounded = true;
        }
        else
        {
            isPlayerGrounded = false;
        }

    }
}
