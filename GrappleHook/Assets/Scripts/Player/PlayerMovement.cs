using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;

    public float speed;

    [SerializeField]
    bool isPlayerGrounded;

    public Vector3 move;

    GrapplingHook hook;

    [SerializeField]
    float jumpHeight;

    int currentNumberOfJumps = 0;
    [SerializeField]
    int maxNumberOfJumps;

    [SerializeField]
    float boostCooldown;

    [SerializeField]
    Vector3 boostForce;

    float timeSinceBoost;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        hook = GetComponent<GrapplingHook>();

        timeSinceBoost = boostCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfGrounded();

        if(!hook.hasHooked || isPlayerGrounded)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * Time.deltaTime;

            rb.MovePosition(transform.position + move);
        }

        //Jump
        if(Input.GetKeyDown(KeyCode.Space) && !hook.hasHooked)
        {
            if(isPlayerGrounded)
            {
                currentNumberOfJumps = 0;
            }
            if(currentNumberOfJumps < maxNumberOfJumps)
            {
                rb.velocity = new Vector3(0, Mathf.Sqrt(jumpHeight * 2f * 9.81f), 0);
                currentNumberOfJumps++;
            }
        }

        //Boost
        timeSinceBoost += Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.LeftShift) && timeSinceBoost >= boostCooldown)
        {
            rb.AddForce(transform.right * boostForce.x + transform.up * boostForce.y + transform.forward * boostForce.z);
            timeSinceBoost = 0;
        }
    }

    void CheckIfGrounded()
    {
        RaycastHit hit;
        float dist = 1.1f;
        Vector3 dir = Vector3.down;

        if (Physics.Raycast(transform.position, dir, out hit, dist))
        {

            if (isPlayerGrounded == false)
            {
                rb.velocity = Vector3.zero;
            }
                isPlayerGrounded = true;
        }
        else
        {
            isPlayerGrounded = false;
        }

    }
}
