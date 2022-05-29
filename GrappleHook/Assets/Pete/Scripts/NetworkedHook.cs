using System.Collections;
using System.Collections.Generic;
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class NetworkedHook : MonoBehaviourPun
//=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
{
    [SerializeField]
    Camera cam;

    [SerializeField]
    public GameObject hook;

    [SerializeField]
    GameObject grappleHook;

    [SerializeField]
    public GameObject hookStartPosition;

    [SerializeField]
    float playerReelInSpeed = 40;

    [SerializeField]
    float playerMoveSpeed = 15;

    [SerializeField]
    float hookMoveSpeed;

    [SerializeField]
    float hookMaxDistance;

    [SerializeField]
    LineRenderer rope;

    [SerializeField]
    float swingVelocity = 40;

    [SerializeField]
    PlayerController playerController;

    [SerializeField]
    Animator anim;

    [SerializeField] AudioClip[] clips;

    bool ropeLengthReachedSwinging;

    float hookCurrentDistance;
    public bool hasHookFired;
    public bool hasHooked;
    public bool hookReturning;
    public GameObject hookedObject = null;

    private bool isPlayerGrounded;

    bool isSwinging;

    Vector3 hookDirection;

    public Rigidbody rbHook;
    Rigidbody rbPlayer;

    public float ropeLength;
    float swingHeight;

    bool leftGround = false;

    Vector3 playerVelocity = Vector3.zero;

    Vector3 previousPosition;
    Vector3 currentPosition;

    GameObject briefCase;
    private SpringJoint joint;

    public bool isReeling;

    bool isJumping;

    float jumpTime = 2;

    [SerializeField]
    GameObject centreDot;

    float grappleSoundTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        rbPlayer = transform.GetComponent<Rigidbody>();
        rbHook = hook.GetComponent<Rigidbody>();
        rope = hook.GetComponent<LineRenderer>();
        briefCase = GameObject.FindGameObjectWithTag("BriefCase");
        centreDot = GameObject.Find("Player UI Dot");
    }

    // Update is called once per frame
    void Update()
    {
        // Draw rope.
        Vector3[] ropePositions = new Vector3[2] { grappleHook.transform.position, hook.transform.position };
        DrawRope(ropePositions);

        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        //Break out of update loop if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return;

        // Break out for the first 1s.
        if (Time.time < 1f)
            return;
        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        if (playerController.jumping)
        {
            isJumping = true;
            jumpTime = 2;
        }

        if (isJumping)
        {
            jumpTime -= Time.deltaTime;
            if (jumpTime <= 0)
            {
                isJumping = false;
                jumpTime = 2;
            }
        }

        //Check if target to fire at is in range
        DistanceCheck();

        CheckIfGrounded();

        currentPosition = transform.position;
        playerVelocity = (currentPosition - previousPosition) / Time.deltaTime;        

        //Fire hook
        if (Input.GetMouseButtonDown(0) && !hasHookFired && !playerController.ledgeGrabbing && StealBriefCase.ownBriefcase == false)
            FireHook();
        //photonView.RPC("FireHook", RpcTarget.All);

        // Determine hook action.
        if (hasHookFired && StealBriefCase.ownBriefcase == false && !playerController.ledgeGrabbing)
        {
            hook.transform.SetParent(null, true);
            hook.transform.localScale = hookStartPosition.transform.lossyScale;
            hook.transform.LookAt(hook.transform.position + hook.transform.position - grappleHook.transform.position);
            anim.SetBool("FiredGrapple", true);
            //photonView.RPC("DrawRope", RpcTarget.All, ropePositions);
            //rope.SetPosition(0, grappleHook.transform.position);
            //rope.SetPosition(1, hook.transform.position);

            hook.GetComponent<SphereCollider>().isTrigger = false;

            // Check if hook is travelling away from player.
            if (!hookReturning && !hasHooked)
            {
                Vector3 hookPosition = hook.transform.position + hookDirection * Time.deltaTime * hookMoveSpeed;
                //rbHook.MovePosition(hookPosition);
                rbHook.velocity = hookDirection * hookMoveSpeed;
                hookCurrentDistance = Vector3.Distance(hookStartPosition.transform.position, hookPosition);
            }

            // Reel hook in when it reaches the max distance.
            if (hookCurrentDistance >= hookMaxDistance || hookReturning)
            {
                ReturnHook();
            }
            // Or when player presses the right mouse button.
            else if (Input.GetMouseButtonUp(0) && !hasHooked)
            {
                ReturnHook();
            }
            // Or when the rope hits an object.
            RaycastHit rayHit;
            LayerMask avoid = LayerMask.GetMask("WraithPlayer", "Hook", "Player");
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(hookStartPosition.transform.position, hook.transform.position - hookStartPosition.transform.position, out rayHit, Vector3.Magnitude(hook.transform.position - hookStartPosition.transform.position), ~avoid))
            {
                if (rayHit.transform.gameObject != hook && !hookReturning)
                {
                    ReturnHook();
                }
            }

            //if (Input.GetMouseButtonDown(0) /*&& briefCase.transform.parent.gameObject != hook*/)
            //{
            //    if (isSwinging)
            //    {
            //        if (gameObject.GetComponent<SpringJoint>() != null)
            //        {
            //            isSwinging = false;
            //            Destroy(gameObject.GetComponent<SpringJoint>());
            //        }
            //    }
            //    isReeling = false;
            //    hook.transform.position = hookStartPosition.transform.position;
            //    BreakHook();
            //    // Cast a ray from screen point
            //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //    // Save the info
            //    RaycastHit hit;
            //    // You successfully hit
            //    if (Physics.Raycast(ray, out hit))
            //    {
            //        // Find the direction to move in
            //        Vector3 dir = hit.point - hook.transform.position;
            //        hookDirection = dir.normalized;
            //    }
            //    else
            //    {
            //        hookDirection = ray.direction.normalized;
            //    }
            //    Vector3 hookPosition = hook.transform.position + hookDirection * Time.deltaTime * hookMoveSpeed;
            //    rbHook.MovePosition(hookPosition);
            //    hasHookFired = true;
            //}
            if (hasHooked)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    BreakHook();
                }
                //Switched Order from old method
                else if (!isPlayerGrounded)
                {
                    SwingPlayer();
                }
                else if (Input.GetMouseButton(0))
                {
                    isReeling = true;
                    ReelPlayer();
                }

                if (rbPlayer.velocity.magnitude > playerMoveSpeed)
                {
                    rbPlayer.velocity = rbPlayer.velocity.normalized * playerMoveSpeed;
                }

                if (Vector3.Distance(hook.transform.position, transform.position) < 0.52f && hasHooked)
                {
                    rbPlayer.velocity = Vector3.zero;
                    if (gameObject.GetComponent<SpringJoint>() != null)
                    {
                        isSwinging = false;
                        Destroy(gameObject.GetComponent<SpringJoint>());
                    }
                }
                else if(hasHookFired && Vector3.Distance(hook.transform.position, transform.position) > 2f)
                {
                    grappleSoundTime -= Time.deltaTime;
                    if (grappleSoundTime <= 0)
                    {
                        grappleSoundTime = 1.3f;
                        AudioSource.PlayClipAtPoint(clips[1], transform.position, playerController.oneShotVolume);
                    }
                }
            }
            else
            {
                
                if (gameObject.GetComponent<SpringJoint>() != null)
                {
                    isSwinging = false;
                    Destroy(gameObject.GetComponent<SpringJoint>());
                }
            }
        }
        else
        {
            anim.SetBool("FiredGrapple", false);
            if (gameObject.GetComponent<SpringJoint>() != null)
            {
                isSwinging = false;
                Destroy(gameObject.GetComponent<SpringJoint>());
            }

            hook.GetComponent<SphereCollider>().isTrigger = true;
            grappleSoundTime = 0;
            hasHookFired = false;
            hasHooked = false;
            //hook.transform.parent = grappleHook.transform;
            hook.transform.SetParent(grappleHook.transform, true);
            hook.transform.position = hookStartPosition.transform.position;
            hook.transform.rotation = hookStartPosition.transform.rotation;
            hook.transform.localScale = hookStartPosition.transform.localScale;
            rope.SetPosition(0, grappleHook.transform.position);
            rope.SetPosition(1, hook.transform.position);

            if (!playerController.ledgeGrabbing && !isPlayerGrounded)
                rbPlayer.useGravity = true;
            isReeling = false;
        }
        previousPosition = transform.position;
    }

    //[PunRPC]
    public void FireHook()
    {
        LayerMask layerMask = LayerMask.NameToLayer("Player") | LayerMask.NameToLayer("Hook") | LayerMask.NameToLayer("WraithPlayer");

        // Cast a ray from screen point
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Save the info
        RaycastHit hit;
        // You successfully hit

        //if (Physics.Raycast(ray, out hit))
        if (Physics.Raycast(ray, out hit, hookMaxDistance, layerMask))
        {
            // Find the direction to move in
            Vector3 dir = hit.point - hook.transform.position;
            hookDirection = dir.normalized;
        }
        else
        {
            hookDirection = ray.direction.normalized;
        }
        Vector3 hookPosition = hook.transform.position + hookDirection * Time.deltaTime * hookMoveSpeed;
        rbHook.velocity = hookDirection.normalized * hookMoveSpeed;
        //rbHook.MovePosition(hookPosition);
        hasHookFired = true;
        isReeling = false;

        AudioSource.PlayClipAtPoint(clips[0], transform.position, playerController.oneShotVolume);
    }

    public void DrawRope(Vector3[] ropePositions)
    {
        rope.SetPosition(0, ropePositions[0]);
        rope.SetPosition(1, ropePositions[1]);
    }

    public void ReturnHook()
    {
        if (isSwinging)
        {
            if (gameObject.GetComponent<SpringJoint>() != null)
            {
                isSwinging = false;
                Destroy(gameObject.GetComponent<SpringJoint>());
            }
        }
        //hookReturning = true;
        if(hasHooked)
            AudioSource.PlayClipAtPoint(clips[2], transform.position, playerController.oneShotVolume);
        hasHooked = false;
        //hook.layer = 10;
        if (!playerController.ledgeGrabbing && !isPlayerGrounded)
            rbPlayer.useGravity = true;
        //Vector3 hookPosition = hook.transform.position + (hookStartPosition.transform.position - hook.transform.position).normalized * Time.deltaTime * hookMoveSpeed;
        //rbHook.velocity = (hookStartPosition.transform.position - hook.transform.position).normalized * hookMoveSpeed;

        //if (Vector3.Distance(hook.transform.position, hookStartPosition.transform.position) < 1f)
        //{
        hook.transform.position = hookStartPosition.transform.position;
        rbHook.velocity = Vector3.zero;
        hasHookFired = false;
        //hookReturning = false;
        hook.layer = 8;
        isReeling = false;       
        //}
    }

    void ReelPlayer()
    {
        //Original method

        //if (isSwinging)
        //{
        //    if (gameObject.GetComponent<SpringJoint>() != null)
        //    {
        //        isSwinging = false;
        //        Destroy(gameObject.GetComponent<SpringJoint>());
        //    }
        //}

        //float x = Input.GetAxis("Horizontal");
        //float z = Input.GetAxis("Vertical");

        //Vector3 move = (transform.right * x + transform.forward * z) * playerMoveSpeed;

        //Vector3 dir = (hook.transform.position - transform.position).normalized * playerReelInSpeed;
        //Vector3 playerPosition = transform.position + dir * Time.deltaTime + move * Time.deltaTime;
        //rbPlayer.velocity = dir + move;
        //rbPlayer.useGravity = false;
        //isSwinging = false;
        //New Method
        //float distanceToHook = Vector3.Distance(transform.position, hook.transform.position);

        //if (distanceToHook < 1)
        //{
        //    hookedObject = null;
        //    //rbPlayer.useGravity = true;
        //    ReturnHook();
        //    return;
        //}

        //if (leftGround)
        //{
        //    float disChange = playerReelInSpeed * Time.deltaTime;
        //    ropeLength = Vector3.Distance(hookStartPosition.transform.position, hook.transform.position) - disChange;

        //    if(ropeLength < 0)
        //    {
        //        ropeLength = 0;
        //    }

        //    joint.maxDistance = ropeLength; /** 0.8f;*/
        //    joint.minDistance = 0;

        //    float x = Input.GetAxis("Horizontal");
        //    float z = Input.GetAxis("Vertical");

        //    Vector3 move = (transform.right * x + transform.forward * z).normalized * swingVelocity;

        //    Vector3 dir = (hook.transform.position - transform.position).normalized * playerReelInSpeed;
        //    //Vector3 playerPosition = transform.position + dir * Time.deltaTime + move * Time.deltaTime;
        //    rbPlayer.velocity = dir + move;

        //}
        //else
        //{
        //    float x = Input.GetAxis("Horizontal");
        //    float z = Input.GetAxis("Vertical");

        //    Vector3 move = (transform.right * x + transform.forward * z) * playerMoveSpeed;

        //    Vector3 dir = (hook.transform.position - transform.position).normalized * playerReelInSpeed;
        //    Vector3 playerPosition = transform.position + dir * Time.deltaTime + move * Time.deltaTime;
        //    //rbPlayer.velocity = dir + move;
        //    rbPlayer.MovePosition(playerPosition);
        //}

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized * swingVelocity;

        Vector3 dir = (hook.transform.position - transform.position).normalized * playerReelInSpeed;
        Vector3 playerPosition = transform.position + dir * Time.deltaTime + move * Time.deltaTime;
        rbPlayer.velocity = dir + move;

        if (rbPlayer.velocity.magnitude > playerMoveSpeed)
        {
            rbPlayer.velocity = rbPlayer.velocity.normalized * playerMoveSpeed;
        }

    }

    void SwingPlayer()
    {
        //float distanceToHook = Vector3.Distance(transform.position, hook.transform.position);

        //if (distanceToHook < 1)
        //{
        //    hookedObject = null;
        //    //rbPlayer.useGravity = true;
        //    ReturnHook();
        //    return;
        //}

        //rbPlayer.useGravity = true;
        if (!isSwinging)
        {
            isSwinging = true;
            ropeLength = Vector3.Distance(transform.position, hook.transform.position);

            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = hook.transform.position;

            joint.maxDistance = ropeLength; /** 0.8f;*/
            joint.minDistance = 0;

            joint.tolerance = 0;
            joint.spring = 1000000f;
            joint.damper = 3f;
            joint.massScale = 1f;
        }
        rbPlayer.useGravity = false;

        

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z).normalized * swingVelocity;

        rbPlayer.velocity = move;

        if (rbPlayer.velocity.magnitude > playerMoveSpeed)
        {
            rbPlayer.velocity = rbPlayer.velocity.normalized * playerMoveSpeed;
        }

        isReeling = true;
        ReelPlayer();

    }

    public void BreakHook()
    {
        hookedObject = null;
        if (!playerController.ledgeGrabbing && !isPlayerGrounded)
            rbPlayer.useGravity = true;
        ReturnHook();
    }


    void CheckIfGrounded()
    {
        // Break out if not owner.
        if (PhotonNetwork.IsMasterClient)
        {
            if (gameObject.GetPhotonView().ViewID != 1001)
            {
                return;
            }
        }
        else if (!gameObject.GetPhotonView().IsMine)
        {
            return;
        }

        RaycastHit hit;
        float dist = 0.71f;
        Vector3 dir = Vector3.down;
        LayerMask layerMask = LayerMask.NameToLayer("Player") | LayerMask.NameToLayer("Hook") | LayerMask.NameToLayer("WraithPlayer");
        if (Physics.BoxCast(transform.position, Vector3.one * 0.3f, dir, out hit, transform.rotation, dist, layerMask))
        {
            isPlayerGrounded = true;
            jumpTime = 2;
        }
        else
        {
            isPlayerGrounded = false;
            if (!leftGround)
                isJumping = true;
            leftGround = true;
        }

        if (hasHooked && leftGround && !isJumping)
        {
            if (isPlayerGrounded)
            {
                BreakHook();
                rbPlayer.velocity = Vector3.zero;
            }
        }

        if (isPlayerGrounded)
        {
            leftGround = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Break out of collision if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return;

        CheckIfGrounded();

        if (hasHooked && leftGround && !playerController.jumping)
        {
            if (isPlayerGrounded)
            {
                BreakHook();
                rbPlayer.velocity = Vector3.zero;
            }
        }

        if (isPlayerGrounded)
        {
            leftGround = false;
        }
    }

    void DistanceCheck()
    {
        LayerMask layerMask = LayerMask.NameToLayer("Player") | LayerMask.NameToLayer("Hook") | LayerMask.NameToLayer("WraithPlayer");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Save the info
        RaycastHit hit;
        // You successfully hit
        if (Physics.Raycast(ray, out hit, hookMaxDistance, layerMask))// && hit.transform.gameObject.layer != LayerMask.NameToLayer("Player") && hit.transform.gameObject.layer != LayerMask.NameToLayer("Hook") && hit.transform.gameObject.layer != LayerMask.NameToLayer("WraithPlayer"))
        {
            centreDot.GetComponent<Image>().color = new Color32(255, 255, 255, 128);
        }
        else
        {
            centreDot.GetComponent<Image>().color = new Color32(255, 0, 0, 128);
        }
    }
}
