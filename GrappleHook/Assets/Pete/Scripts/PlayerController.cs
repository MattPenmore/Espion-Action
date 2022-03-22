///========================
/// Peter Phillips, 2022
/// PlayerController.cs
///========================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Camera playerCam;
    [SerializeField] Renderer body;
    [SerializeField] AudioListener audioListener;
    [SerializeField] Rigidbody _rb;
    GameObject[] spawnPoints = null;

    [PunRPC]
    private void Initialise(int playerID)
    {
        // Move player to spawn point.
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        transform.position = spawnPoints[playerID].transform.position;
        //transform.position = GameObject.Find("SpawnPoint").transform.position + transform.right * (playerID * 2);

        // Change player colour.
        Color[] playerColours = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };
        //GetComponent<Renderer>().material.color = playerColours[playerID];
        body.material.color = playerColours[playerID];

        Debug.Log("Initialising object with ViewID: " + gameObject.GetPhotonView().ViewID);

        // Disable scene camera and audio listener.
        GameObject sceneCam = GameObject.Find("SceneCamera");
        sceneCam.GetComponent<Camera>().enabled = false;
        sceneCam.GetComponent<AudioListener>().enabled = false;

        // Enable player camera, audio listener, and gravity.
        if (PhotonNetwork.IsMasterClient)
        {
            if (gameObject.GetPhotonView().ViewID == 1001)
            {
                playerCam.enabled = true;
                audioListener.enabled = true;
                _rb.useGravity = true;
            }
        }
        else if (gameObject.GetPhotonView().IsMine)
        {
            playerCam.enabled = true;
            audioListener.enabled = true;
            _rb.useGravity = true;
        }
    }

    Rigidbody rb;

    public float speed;

    [SerializeField]
    bool isPlayerGrounded;

    public Vector3 move;

    NetworkedHook hook;

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
        hook = GetComponent<NetworkedHook>();
        playerUpgrades = GetComponent<PlayerUpgrades>();

        timeSinceBoost = boostCooldown;
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

        if (!hook.hasHooked || isPlayerGrounded)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * speedUpgradeValue * Time.fixedDeltaTime;

            rb.MovePosition(transform.position + move);
        }

        //Jump
        if (Input.GetKeyDown(KeyCode.Space) && !hook.hasHooked)
        {
            if (isPlayerGrounded)
            {
                currentNumberOfJumps = 0;
            }
            if (currentNumberOfJumps < maxNumberOfJumps)
            {
                rb.velocity = new Vector3(0, Mathf.Sqrt(jumpHeight * 2f * 9.81f * jumpUpgradeValue), 0);
                currentNumberOfJumps++;
            }
        }

        //Boost
        timeSinceBoost += Time.fixedDeltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift) && timeSinceBoost >= boostCooldown)
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

//// Old update function for cube test.
//void Update()
//{
//    // Break out of update loop if not the owner of this gameobject.
//    if (!gameObject.GetPhotonView().IsMine)
//        return;
//
//    // Side-to-side movement.da
//    if (Input.GetAxis("X-Axis") != 0)
//        transform.position += Vector3.right * Input.GetAxis("X-Axis") * Time.deltaTime * 10f;
//
//    // Forwards and backwards movement.
//    if (Input.GetAxis("Z-Axis") != 0)
//        transform.position += Vector3.forward * Input.GetAxis("Z-Axis") * Time.deltaTime * 10f;
//
//    // Up and down movement.
//    if (Input.GetAxis("Y-Axis") != 0)
//        transform.position += Vector3.up * Input.GetAxis("Y-Axis") * Time.deltaTime * 10f;
//
//    //transform.position = new Vector3(Mathf.Clamp(transform.position.x, -7f, 7f), Mathf.Clamp(transform.position.y, -3.5f, 3f), 0);
//}
