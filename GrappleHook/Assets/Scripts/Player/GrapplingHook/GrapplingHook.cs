using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [SerializeField]
    Camera cam;

    [SerializeField]
    GameObject hook;

    [SerializeField]
    GameObject grappleHook;

    [SerializeField]
    GameObject hookStartPosition;

    [SerializeField]
    float playerReelInSpeed;

    [SerializeField]
    float playerMoveSpeed;

    [SerializeField]
    float hookMoveSpeed;

    [SerializeField]
    float playerEndForce;

    [SerializeField]
    float hookMaxDistance;

    [SerializeField]
    LineRenderer rope;

    [SerializeField]
    float swingVelocity;

    float hookCurrentDistance;
    bool hasHookFired;
    public bool hasHooked;
    bool hookReturning;
    public GameObject hookedObject = null;

    private bool isPlayerGrounded;

    bool isSwinging;

    Vector3 hookDirection;

    Rigidbody rbHook;
    Rigidbody rbPlayer;

    public float ropeLength;
    float swingHeight;

    bool leftGround = false;

    Vector3 playerVelocity = Vector3.zero;

    Vector3 previousPosition;
    Vector3 currentPosition;

    GameObject briefCase;

    private SpringJoint joint;
    // Start is called before the first frame update
    void Start()
    {
        rbPlayer = transform.GetComponent<Rigidbody>();
        rbHook = hook.GetComponent<Rigidbody>();
        rope = hook.GetComponent<LineRenderer>();
        briefCase = GameObject.FindGameObjectWithTag("BriefCase");
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfGrounded();

        currentPosition = transform.position;
        playerVelocity = (currentPosition - previousPosition) / Time.deltaTime;
        

        hook.transform.parent = null;
        //Fire hook
        if (Input.GetMouseButtonDown(0) && !hasHookFired)
        {
            // Cast a ray from screen point
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Save the info
            RaycastHit hit;
            // You successfully hit
            if (Physics.Raycast(ray,out hit))
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
            rbHook.MovePosition(hookPosition);
            hasHookFired = true;
        }

        if(hasHookFired && GetComponent<StealBriefCase>().ownBriefcase == false)
        {
            rope.SetPosition(0, grappleHook.transform.position);
            rope.SetPosition(1, hook.transform.position);
            if (!hookReturning && !hasHooked)
            {
                Vector3 hookPosition = hook.transform.position + hookDirection * Time.deltaTime * hookMoveSpeed;
                //rbHook.MovePosition(hookPosition);
                rbHook.velocity = hookDirection * hookMoveSpeed;
                hookCurrentDistance = Vector3.Distance(hookStartPosition.transform.position, hookPosition);
            }

            if(hookCurrentDistance >= hookMaxDistance || hookReturning)
            {
                ReturnHook();
            }
            else if (Input.GetMouseButtonDown(2) && !hasHooked)
            {
                ReturnHook();
            }

            RaycastHit rayHit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(hookStartPosition.transform.position, hook.transform.position - hookStartPosition.transform.position, out rayHit, Mathf.Infinity))
            {
                if(rayHit.transform.gameObject != hook && !hookReturning)
                {
                    ReturnHook();
                }
            }

            if (Input.GetMouseButtonDown(0) && briefCase.transform.parent.gameObject != hook)
            {
                if (isSwinging)
                {
                    if (gameObject.GetComponent<SpringJoint>() != null)
                    {
                        isSwinging = false;
                        Destroy(gameObject.GetComponent<SpringJoint>());
                    }
                }

                hook.transform.position = hookStartPosition.transform.position;
                BreakHook();
                // Cast a ray from screen point
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Save the info
                RaycastHit hit;
                // You successfully hit
                if (Physics.Raycast(ray, out hit))
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
                rbHook.MovePosition(hookPosition);
                hasHookFired = true;
            }
            else if(hasHooked)
            {
                if (Input.GetMouseButtonDown(2))
                {
                    BreakHook();
                }
                else if (Input.GetMouseButton(1))
                {
                    ReelPlayer();
                }
                else if(!isPlayerGrounded)
                {
                    SwingPlayer();
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
            if (gameObject.GetComponent<SpringJoint>() != null)
            {
                isSwinging = false;
                Destroy(gameObject.GetComponent<SpringJoint>());
            }
            hasHookFired = false;
            hook.transform.parent = grappleHook.transform;
            hook.transform.position = hookStartPosition.transform.position;
            rope.SetPosition(0, grappleHook.transform.position);
            rope.SetPosition(1, hook.transform.position);
        }
        previousPosition = transform.position;
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
        hookReturning = true;
        hasHooked = false;
        //Vector3 hookPosition = hook.transform.position + (hookStartPosition.transform.position - hook.transform.position).normalized * Time.deltaTime * hookMoveSpeed;
        rbHook.velocity = (hookStartPosition.transform.position - hook.transform.position).normalized * hookMoveSpeed;

        if (Vector3.Distance(hook.transform.position, hookStartPosition.transform.position) < 1f)
        {
            hook.transform.position = hookStartPosition.transform.position;
            rbHook.velocity = Vector3.zero;
            hasHookFired = false;
            hookReturning = false;
            
        }
    }

    void ReelPlayer()
    {
        if (isSwinging)
        {
            if (gameObject.GetComponent<SpringJoint>() != null)
            {
                isSwinging = false;
                Destroy(gameObject.GetComponent<SpringJoint>());
            }
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z) * playerMoveSpeed;

        Vector3 dir = (hook.transform.position - transform.position).normalized * playerReelInSpeed;
        Vector3 playerPosition = transform.position + dir * Time.deltaTime + move * Time.deltaTime;
        rbPlayer.velocity = dir + move;
        rbPlayer.useGravity = false;
        isSwinging = false;
        float distanceToHook = Vector3.Distance(transform.position, hook.transform.position);
        if (distanceToHook < 1)
        {
            CheckIfGrounded();
            if(!isPlayerGrounded)
            {
                //rbPlayer.AddForce((dir + move) * playerEndForce);
            }
            hookedObject = null;
            rbPlayer.useGravity = true;
            ReturnHook();         
        }
    }

    void SwingPlayer()
    {
        //rbPlayer.useGravity = true;
        if(!isSwinging)
        {
            isSwinging = true;
            ropeLength = Vector3.Distance(hookStartPosition.transform.position, hook.transform.position);

            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = hook.transform.position;

            joint.maxDistance = ropeLength; /** 0.8f;*/
            joint.minDistance = ropeLength;

            joint.tolerance = 0;
            joint.spring = 50f;
            joint.damper = 7f;
            joint.massScale = 1f;
        }
        rbPlayer.useGravity = true;


        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward *z) * swingVelocity;
        rbPlayer.AddForce(move, ForceMode.Acceleration);
        if (rbPlayer.velocity.magnitude > playerMoveSpeed)
        {
            rbPlayer.velocity = rbPlayer.velocity.normalized * playerMoveSpeed;
        }
    }

    void BreakHook()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z) * playerMoveSpeed;
        Vector3 dir = Vector3.zero;

        if (!isSwinging)
        {
            dir = (hook.transform.position - transform.position).normalized * playerReelInSpeed;
        }

        CheckIfGrounded();
        if (!isPlayerGrounded)
        {
           //rbPlayer.AddForce((dir + move) * playerEndForce);
        }
        hookedObject = null;
        rbPlayer.useGravity = true;
        ReturnHook();
    }


    void CheckIfGrounded()
    {
        RaycastHit hit;
        float dist = 1.1f;
        Vector3 dir = Vector3.down;

        if(Physics.Raycast(transform.position, dir, out hit, dist))
        {
            isPlayerGrounded = true;
        }
        else
        {
            isPlayerGrounded = false;
            leftGround = true;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckIfGrounded();

        if (hasHooked && leftGround)
        {
            if (isPlayerGrounded)
            {
                BreakHook();
            }
        }

        if(isPlayerGrounded)
        {
            leftGround = false;
        }
    }
}
