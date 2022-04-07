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

    public int currentNumberOfJumps = 0;
    [SerializeField]
    int maxNumberOfJumps;

    [SerializeField]
    float boostCooldown;

    [SerializeField]
    Vector3 boostForce;

    PlayerUpgrades playerUpgrades;
    float jumpUpgradeValue;
    float speedUpgradeValue;

    float timeSinceBoost;

    bool jumping = false;
    float jumpTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        hook = GetComponent<GrapplingHook>();
        playerUpgrades = GetComponent<PlayerUpgrades>();

        timeSinceBoost = boostCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if(jumping)
        {
            jumpTime -= Time.deltaTime;

            if(jumpTime <= 0)
            {
                jumping = false;
                jumpTime = 0.5f;
            }
        }
        //Upgrades to jumping and speed
        if (playerUpgrades.hasUpgrade)
        {
            if (playerUpgrades.currentUpgrade == "JumpBoost")
            {
                jumpUpgradeValue = playerUpgrades.jumpUpgradeValue;
            }
            else
            {
                jumpUpgradeValue = 1;
            }

            if (playerUpgrades.currentUpgrade == "SpeedBoost")
            {
                jumpUpgradeValue = playerUpgrades.speedUpgradeValue;
            }
            else
            {
                speedUpgradeValue = 1;
            }
        }
        else
        {
            jumpUpgradeValue = 1;
            speedUpgradeValue = 1;
        }


        CheckIfGrounded();

        if(isPlayerGrounded)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * speedUpgradeValue/* * Time.deltaTime*/;

            rb.velocity = move;
            //rb.MovePosition(transform.position + move);
        }
        else if(!hook.hasHooked)
        {

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * speedUpgradeValue * Time.deltaTime;
            float magnitudeTotal = new Vector3(rb.velocity.x + move.x, 0, rb.velocity.z + move.z).magnitude;
            float magnitudeVel = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

            if(magnitudeTotal > magnitudeVel && magnitudeTotal > speed * speedUpgradeValue)
            {
                rb.velocity = new Vector3(rb.velocity.x + move.x, 0 , rb.velocity.z + move.z).normalized * magnitudeVel + new Vector3(0, rb.velocity.y, 0);
            }
            else
            {
                rb.velocity = rb.velocity + move;
            }
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
                rb.velocity += new Vector3(0, Mathf.Sqrt(jumpHeight * 2f * 9.81f * jumpUpgradeValue), 0);
                currentNumberOfJumps++;
                jumping = true;
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
        float dist = 0.6f;
        Vector3 dir = Vector3.down;

        if (Physics.SphereCast(transform.position, 0.5f ,dir, out hit, dist) /*&& !jumping*/)
        {

            if (isPlayerGrounded == false)
            {
                rb.velocity = Vector3.zero;
                currentNumberOfJumps = 0;
            }
                isPlayerGrounded = true;
        }
        else
        {
            isPlayerGrounded = false;
        }

    }
}
