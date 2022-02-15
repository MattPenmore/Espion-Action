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

    PlayerUpgrades playerUpgrades;
    float jumpUpgradeValue;
    float speedUpgradeValue;

    float timeSinceBoost;
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

        if(!hook.hasHooked || isPlayerGrounded)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * speedUpgradeValue * Time.deltaTime;

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
                rb.velocity = new Vector3(0, Mathf.Sqrt(jumpHeight * 2f * 9.81f * jumpUpgradeValue), 0);
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
