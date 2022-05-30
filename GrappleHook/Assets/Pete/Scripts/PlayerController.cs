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
    public GameObject hookObject;
    [SerializeField] NetworkedHook netHook;
    [SerializeField] Camera playerCam;
    [SerializeField] SkinnedMeshRenderer body;
    [SerializeField] Renderer hookMaterial;
    [SerializeField] Renderer grapplingGunMaterial;
    [SerializeField] AudioListener audioListener;
    [SerializeField] AudioClip[] clips;
    GameObject[] spawnPoints = null;

    public GameObject spawnPoint;
    Vector3 targetPosition;

    public bool ledgeGrabbing;

    [SerializeField]
    Vector3 ledgeGrabTarget;

    [SerializeField]
    Animator anim;

    public bool respawning;
    public bool playedRespawnSound;

    internal float localTime = 0;

    float ledgeGrabTime = 0;
    float maxLedgeGrabTime = 3;
    private StealBriefCase stealBriefCase;

    internal float oneShotVolume = .4f;

    private TutorialScript tutorialScript;
    //private Transform startGameButton;
    private bool inTutorial;

    [PunRPC]
    private void Initialise(int playerID, bool inTutorial)
    {
        this.inTutorial = inTutorial;

        // Move player to spawn point.
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnPoint = spawnPoints[playerID];
        transform.position = spawnPoint.transform.position;
        transform.forward = spawnPoint.transform.forward;

        // Change player colour.
        Color[] playerColours = new Color[] { Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };
        //GetComponent<Renderer>().material.color = playerColours[playerID];
        //body.material.color = playerColours[playerID];
        hookMaterial.material.color = playerColours[playerID];
        grapplingGunMaterial.material.color = playerColours[playerID];
        hookObject.GetComponent<LineRenderer>().startColor = playerColours[playerID];
        hookObject.GetComponent<LineRenderer>().endColor = playerColours[playerID];

        Debug.Log("Initialising object with ViewID: " + gameObject.GetPhotonView().ViewID);

        // Disable spec cam, Enable player UI, camera, audio listener, and gravity.
        if (PhotonNetwork.IsMasterClient)
        {
            if (gameObject.GetPhotonView().ViewID == 1001 && inTutorial || gameObject.GetPhotonView().ViewID == 1001 + 2 * PhotonNetwork.PlayerList.Length && !inTutorial)
            {
                DisableSpecCam();
                EnablePlayerUI();
        
                playerCam.enabled = true;
                audioListener.enabled = true;
                Debug.Log("SETTING inTutorial = " + inTutorial);
            }
            else
            {
                RemoveComponents();
            }
            
        }
        else if (gameObject.GetPhotonView().IsMine)
        {
            DisableSpecCam();
            EnablePlayerUI();
           
            playerCam.enabled = true;
            audioListener.enabled = true;
        }
        else
        {
            RemoveComponents();
        }
        stealBriefCase = GetComponent<StealBriefCase>();
        stealBriefCase.inTutorial = inTutorial;

        //if (inTutorial && PhotonNetwork.IsMasterClient)
        //{
        //    tutorialScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TutorialScript>();
        //    startGameButton = tutorialScript.startGameButton.transform;
        //}
    }
    private void RemoveComponents()
    {
        //Destroy(GetComponent<Rigidbody>());
    }

    private void DisableSpecCam()
    {
        // Disable scene camera and audio listener.
        GameObject sceneCam = GameObject.FindGameObjectWithTag("SpecCam");
        if (sceneCam != null)
            sceneCam.SetActive(false);        
    }

    private void EnablePlayerUI()
    {
        // Enable the player UI.
        GameObject playerUI = GameObject.FindGameObjectWithTag("PlayerUI");

        if (playerUI != null)
        {
            Canvas[] canvasses = playerUI.GetComponentsInChildren<Canvas>();
            foreach (Canvas c in canvasses)
                c.enabled = true;
        }
    }    

    Rigidbody rb;
    public float speed;

    [SerializeField]
    bool isPlayerGrounded;

    public Vector3 move;

    NetworkedHook hook;

    [SerializeField] GameObject playerCamObject;
    [SerializeField] Transform[] cameraPositions;

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
    public bool jumping = false;
    float jumpTime = 0.5f;

    float stepTimeAudio = 0f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //Break out of start if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return;

        hook = GetComponent<NetworkedHook>();
        playerUpgrades = GetComponent<PlayerUpgrades>();

        timeSinceBoost = boostCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        //Switch gravity on/off and then break out of update loop if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
        {
            CheckIfGrounded();
            return;
        }

        // Break out for the first 1s.
        if (localTime < 1f)
        {
            localTime += Time.deltaTime;
            return;
        }

        targetPosition = transform.position;

        // Camera control.
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Swap between first and third person.
            if (playerCamObject.transform.localPosition == cameraPositions[0].localPosition) // first person
            {
                playerCamObject.transform.localPosition = cameraPositions[1].localPosition;
                stealBriefCase.firstPerson = false;
            }
            else
            {
                playerCamObject.transform.localPosition = cameraPositions[0].localPosition;
                stealBriefCase.firstPerson = true;
            }
            // Swap between first and third person.
            //playerCamObject.transform.localPosition = (playerCamObject.transform.localPosition == cameraPositions[0].localPosition) ? cameraPositions[1].localPosition : cameraPositions[0].localPosition;
        }


        if (jumping)
        {
            jumpTime -= Time.deltaTime;

            if (jumpTime <= 0)
            {
                jumping = false;
                anim.SetBool("isJumping", false);
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

        if (isPlayerGrounded && !hook.hasHooked)
        {
            anim.SetBool("IsGrounded", true);
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            move = (transform.right * x + transform.forward * z).normalized * speed * speedUpgradeValue/* * Time.deltaTime*/;

            rb.velocity = move;
            if (move.magnitude != 0)
            {
                anim.SetBool("isRunning", true);
                stepTimeAudio -= Time.deltaTime;
                if(stepTimeAudio <= 0)
                {
                    AudioSource.PlayClipAtPoint(clips[0], transform.position, oneShotVolume);
                    stepTimeAudio = 0.4f;
                }
                //if(GetComponent<AudioSource>().clip != clips[0] || !GetComponent<AudioSource>().isPlaying)
                //{
                //    GetComponent<AudioSource>().clip = clips[0];
                //    GetComponent<AudioSource>().Play();
                //}
            }
            else
                anim.SetBool("isRunning", false);

            if (x < 0)
                anim.SetBool("RunLeft", true);
            else
                anim.SetBool("RunLeft", false);

            if ( x > 0)
                anim.SetBool("RunRight", true);
            else
                anim.SetBool("RunRight", false);

            if (z < 0)
                anim.SetBool("RunBack", true);
            else
                anim.SetBool("RunBack", false);

            //rb.MovePosition(transform.position + move);
        }
        else if ((!hook.hasHooked || jumping) && !ledgeGrabbing)
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("RunLeft", false);
            anim.SetBool("RunRight", false);
            anim.SetBool("IsGrounded", false);
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            move = (transform.right * x + transform.forward * z) * speed * 3 * speedUpgradeValue * Time.deltaTime;
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
        else
        {
            anim.SetBool("IsGrounded", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("RunLeft", false);
            anim.SetBool("RunRight", false);
        }

        //Jump
        if (Input.GetKeyDown(KeyCode.Space) && !ledgeGrabbing /*&& !hook.hasHooked*/)
        {
            if (isPlayerGrounded)
            {
                currentNumberOfJumps = 0;
            }
            if (currentNumberOfJumps < maxNumberOfJumps)
            {
                rb.velocity += new Vector3(0, Mathf.Sqrt(jumpHeight * 2f * -Physics.gravity.y * jumpUpgradeValue), 0);
                jumping = true;
                jumpTime = 0.5f;
                anim.SetBool("isJumping", true);
                AudioSource.PlayClipAtPoint((currentNumberOfJumps == 0) ? clips[1] : clips[3], transform.position, oneShotVolume);
                currentNumberOfJumps++;
            }
        }

        //Boost
        timeSinceBoost += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift) && timeSinceBoost >= boostCooldown && !ledgeGrabbing)
        { 
            rb.AddForce(transform.right * boostForce.x + transform.up * boostForce.y + transform.forward * boostForce.z);
            timeSinceBoost = 0;
            AudioSource.PlayClipAtPoint(clips[3], transform.position, oneShotVolume);
            
        }

        if(isPlayerGrounded && !jumping && !netHook.isReeling)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 50 * Time.deltaTime);
        }

        //Ledge Grab
        if (!isPlayerGrounded && !ledgeGrabbing && /*!hook.hasHooked &&*/ !jumping)
        {
            LayerMask avoid = LayerMask.GetMask("WraithPlayer", "Hook", "Player");
            RaycastHit hitHoriz;
            RaycastHit hitVert;
            RaycastHit hitAbove;
            Vector3 vertPos = transform.position + transform.forward * 1.32f + transform.up * 2;
            Vector3 horizPos = transform.position;
            horizPos.y += 1; 
            if (Physics.BoxCast(transform.position, new Vector3(0.3f,1f, 0.1f), transform.forward, out hitHoriz, transform.rotation, 2f, ~avoid) && Physics.BoxCast(vertPos, Vector3.one * 0.5f, -Vector3.up, out hitVert, transform.rotation, 2f, ~avoid) && !Physics.BoxCast(transform.position, Vector3.one * 0.2f, Vector3.up, out hitAbove, transform.rotation, /*0.1f + Mathf.Abs(transform.position.y - ledgeGrabTarget.y)*/2.8f, ~avoid))
            {
                RaycastHit[] vertCheck = Physics.BoxCastAll(vertPos, Vector3.one * 0.5f, -Vector3.up, transform.rotation, 2f, ~avoid);
                foreach(RaycastHit vert in vertCheck)
                {

                    ledgeGrabTarget = vert.point;
                    ledgeGrabTarget.y += 1;

                    Collider[] hitColliders = Physics.OverlapBox(ledgeGrabTarget, new Vector3(0.49f, 0.98f, 0.49f), transform.rotation);
                    if (hitColliders.Length == 0)
                    {
                        ledgeGrabbing = true;
                        rb.useGravity = false;
                        rb.velocity = Vector3.zero;
                        break;
                    }
                }

            }
        }

        //RaycastHit hitAboveCheck;
        LayerMask avoidCheck = LayerMask.GetMask("WraithPlayer", "Hook", "Player");
        if (Mathf.Abs(Vector3.Magnitude(transform.position - ledgeGrabTarget)) > 5 || ledgeGrabTime > maxLedgeGrabTime /*|| !Physics.BoxCast(transform.position, Vector3.one * 0.2f, Vector3.up, out hitAboveCheck, transform.rotation, 0.1f + Mathf.Abs(transform.position.y - ledgeGrabTarget.y), ~avoidCheck)*/)
        {
            ledgeGrabbing = false;
        }

        if(ledgeGrabbing)
        {
            if (netHook.hasHookFired)
                netHook.BreakHook();

            ledgeGrabTime += Time.deltaTime;
            anim.SetBool("LedgeGrabbing", true);
            if(ledgeGrabTarget.y > (transform.position.y + 0.4f))
                rb.velocity = Vector3.up * 6;
            else
                rb.velocity = (ledgeGrabTarget - transform.position).normalized * 6;
            //transform.position = Vector3.Lerp(transform.position, ledgeGrabTarget, 5 * Time.deltaTime);
            rb.useGravity = false;
            if (isPlayerGrounded /*|| rb.velocity.y < 0.1f*/)
            {
                ledgeGrabbing = false;
                //rb.velocity = Vector3.zero;
            }
        }
        else
        {
            ledgeGrabTime = 0;
            anim.SetBool("LedgeGrabbing", false);
            if (!isPlayerGrounded && !netHook.hasHooked)
                rb.useGravity = true;

            ledgeGrabbing = false;
        }

        //// Check if in tutorial and if we are client.
        //if (inTutorial && PhotonNetwork.IsMasterClient)
        //{
        //    // Turn off interact canvas.
        //    tutorialScript.interactCanvas.enabled = false;
        //    // Check if we are close to button.
        //    if (Vector3.Distance(transform.position, startGameButton.position) < 5)
        //    {
        //        LayerMask layerMask = LayerMask.GetMask("Button");
        //        // Raycast to see if we are looking at the button
        //        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, 5f, layerMask, QueryTriggerInteraction.Collide))
        //        {
        //            // Turn on interact canvas.
        //            tutorialScript.interactCanvas.enabled = true;
        //            // Start game if interact pressed.
        //            if (Input.GetKeyDown(KeyCode.E))
        //                tutorialScript.gameObject.GetPhotonView().RPC("StartGame", RpcTarget.AllBuffered);
        //        }
        //    }
        //}
        if(respawning && !playedRespawnSound)
        {
            AudioSource.PlayClipAtPoint(clips[4], transform.position, oneShotVolume);
            playedRespawnSound = true;
        }

        MapDistanceCheck();
    }

    void CheckIfGrounded()
    {
        RaycastHit hit;
        float dist = 0.75f;
        Vector3 dir = Vector3.down;

        if (Physics.BoxCast(transform.position, Vector3.one * 0.3f, dir, out hit, transform.rotation, dist) && !jumping)
        {
            Vector3 rayCastPoint = hit.point;
            targetPosition.y = rayCastPoint.y + 1;
            if (isPlayerGrounded == false)
            {
                rb.velocity = Vector3.zero;
                currentNumberOfJumps = 0;
                AudioSource.PlayClipAtPoint(clips[2], transform.position, oneShotVolume);
            }
            isPlayerGrounded = true;
            rb.useGravity = false;
        }
        else
        {
            isPlayerGrounded = false;
            if (!netHook.hasHooked)
                rb.useGravity = true;
        }
    }

    void MapDistanceCheck()
    {
        float dist = transform.position.magnitude;
        if (dist > 200 && !respawning)
            FindObjectOfType<Respawn>().RespawnPlayerPush(gameObject);
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
