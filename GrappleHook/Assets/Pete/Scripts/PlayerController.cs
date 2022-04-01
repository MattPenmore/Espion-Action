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

    public GameObject spawnPoint;

    [PunRPC]
    private void Initialise(int playerID)
    {
        // Move player to spawn point.
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoint = spawnPoints[playerID];
        transform.position = spawnPoint.transform.position;
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
            else
            {
                RemoveComponents();
            }
        }
        else if (gameObject.GetPhotonView().IsMine)
        {
            playerCam.enabled = true;
            audioListener.enabled = true;
            _rb.useGravity = true;
        }
        else
        {
            RemoveComponents();
        }
    }
    private void RemoveComponents()
    {
        Destroy(GetComponent<Rigidbody>());
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
    bool jumping = false;
    float jumpTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        //Break out of start if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return;

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

        if (jumping)
        {
            jumpTime -= Time.deltaTime;

            if (jumpTime <= 0)
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

        if (isPlayerGrounded)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * speedUpgradeValue/* * Time.deltaTime*/;

            rb.velocity = move;
            //rb.MovePosition(transform.position + move);
        }
        else if (!hook.hasHooked)
        {

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * speedUpgradeValue * Time.deltaTime;
            float magnitudeTotal = new Vector3(rb.velocity.x + move.x, 0, rb.velocity.z + move.z).magnitude;
            float magnitudeVel = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

            if (magnitudeTotal > magnitudeVel && magnitudeTotal > speed * speedUpgradeValue)
            {
                rb.velocity = new Vector3(rb.velocity.x + move.x, 0, rb.velocity.z + move.z).normalized * magnitudeVel + new Vector3(0, rb.velocity.y, 0);
            }
            else
            {
                rb.velocity = rb.velocity + move;
            }
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
                rb.velocity += new Vector3(0, Mathf.Sqrt(jumpHeight * 2f * 9.81f * jumpUpgradeValue), 0);
                currentNumberOfJumps++;
                jumping = true;
            }
        }

        //Boost
        timeSinceBoost += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift) && timeSinceBoost >= boostCooldown)
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

        if (Physics.SphereCast(transform.position, 0.5f, dir, out hit, dist) && !jumping)
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
