using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class StealBriefCase : MonoBehaviourPun
{
    public static float winTime = 60f;
    public static bool ownBriefcase;
    public bool inTutorial;

    public bool firstPerson = true;
    public GameObject briefCase;
    private GameObject gameManager;
    private GameObject playerTimerText;
    //private GameObject otherPlayerTimerText;
    private Color[] playerColours;
    private GameObject briefcaseOutline;

    float maxStealTime = 1f;

    bool stealingBriefCase;
    public static float ownedTime;
    public static float currentPlayerOwnedTime;
    float stealTimer;
    bool gameOver;

    [SerializeField]
    float stealDistance;

    [SerializeField]
    GameObject briefCaseLocationXZ;

    [SerializeField]
    GameObject briefCaseLocationY;

    [SerializeField]
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        stealTimer = 0f;
        ownedTime = winTime;
        currentPlayerOwnedTime = winTime;
        gameOver = false;
        stealingBriefCase = false;
        ownBriefcase = false;
        briefCase = GameObject.FindGameObjectWithTag("BriefCase");
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        playerTimerText = GameObject.FindGameObjectWithTag("PlayerTimerText");
        //otherPlayerTimerText = GameObject.FindGameObjectWithTag("OtherPlayerTimerText");
        briefcaseOutline = GameObject.FindGameObjectWithTag("Outline");
        playerColours = new Color[] { Color.black, Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };
    }

    // Update is called once per frame
    void Update()
    {
        //Break out of update loop if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return;

        if (ownedTime > winTime)
            ownedTime = winTime;

        //Debug.Log(briefcaseOutline.GetComponent<Renderer>().material.color + " | " + playerColours[0]);
        if (!ownBriefcase && briefcaseOutline.GetComponent<Renderer>().material.color != playerColours[0] && !inTutorial) //black
        {
            currentPlayerOwnedTime -= Time.deltaTime;
            playerTimerText.GetComponent<Text>().text = (currentPlayerOwnedTime).ToString("F0");
        }


        if (ownBriefcase && !gameOver)
        {
            if (playerTimerText != null)
                playerTimerText.GetComponent<Text>().text = (ownedTime).ToString("F0");
            anim.SetBool("HasBriefCase", true);
            ownedTime -= Time.deltaTime;
            if(ownedTime <= 0 && !inTutorial)
            {
                gameOver = true;
                Debug.Log("Win");
                Vector4 pColour = GetComponentInChildren<Renderer>().material.color;
                float[] playerColour = { pColour.x, pColour.y, pColour.z, pColour.w };
                gameManager.GetPhotonView().RPC("EndGame", RpcTarget.All, photonView.Owner.NickName, playerColour);
            }
        }
        else if(briefCase.GetComponent<BriefCase>().stealable == true)
        {
            anim.SetBool("HasBriefCase", false);
            if (Vector3.Distance(transform.position, briefCase.transform.position) <= stealDistance && Input.GetKeyDown(KeyCode.E))
            {
                //if(briefCase.transform.parent)
                //{
                //    if(briefCase.transform.parent.tag == "Player")
                //    {
                //        briefCase.transform.parent.GetComponent<StealBriefCase>().ownBriefcase = false;
                //    }
                //}
                //briefCase.transform.parent = null;


                //briefCase.GetPhotonView().RequestOwnership();
                //photonView.RPC("BriefcaseStolen", RpcTarget.All);

                CallBreifcaseTransfer(photonView.OwnerActorNr, true, ownedTime);
                //photonView.RPC(nameof(TransferBriefcase), RpcTarget.MasterClient, photonView.OwnerActorNr);
                
                //stealingBriefCase = true;
                //ownBriefcase = true;

                //Debug.Log("ownBriefcase = true;");
            }
        }
        else
            anim.SetBool("HasBriefCase", false);
        Vector3 briefCasePosition = new Vector3(briefCaseLocationXZ.transform.position.x, briefCaseLocationY.transform.position.y, briefCaseLocationXZ.transform.position.z);
        if (stealingBriefCase)
        {
            briefCase.transform.parent = null;
            stealTimer += Time.deltaTime;
            float briefcaseDistance = (briefCase.transform.position - briefCasePosition).magnitude;
            briefCase.transform.position = Vector3.Lerp(briefCase.transform.position, briefCasePosition, Mathf.Max(20, 20 * briefcaseDistance) * Time.deltaTime);
            briefCase.transform.rotation = Quaternion.Lerp(briefCase.transform.rotation, briefCaseLocationXZ.transform.localRotation, 1);

            if(Vector3.Distance(briefCase.transform.position, briefCasePosition) < 0.1f || stealTimer >= maxStealTime)
            {
                briefCase.transform.position = briefCasePosition;
                briefCase.transform.rotation = briefCaseLocationXZ.transform.localRotation;
                briefCase.transform.parent = transform;
                stealingBriefCase = false;
                stealTimer = 0f;
            }
        }
        else if(ownBriefcase && !gameOver)
        {
            briefCase.transform.parent = transform;
            briefCase.transform.position = briefCasePosition;
            briefCase.transform.localRotation = briefCaseLocationXZ.transform.localRotation;
        }

        if(ownBriefcase)
        {
            if (firstPerson)
                foreach(MeshRenderer renderer in briefCase.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.enabled = false;
                }
            else
                foreach (MeshRenderer renderer in briefCase.GetComponentsInChildren<MeshRenderer>())
                {
                    renderer.enabled = true;
                }
        }
        else
            foreach(MeshRenderer renderer in briefCase.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = true;
            }
    }

    public void CallBreifcaseTransfer(int actorNo, bool transferOwner, float currentOwnedTime)
    {
        photonView.RPC(nameof(TransferBriefcase), RpcTarget.MasterClient, actorNo, transferOwner, currentOwnedTime);
    }

    [PunRPC]
    public void TransferBriefcase(int actorNo, bool transferOwner, float currentOwnedTime)
    {
        if (transferOwner)
            briefCase.GetPhotonView().TransferOwnership(actorNo);

        photonView.RPC(nameof(BriefcaseStolen), RpcTarget.AllBufferedViaServer, actorNo, currentOwnedTime);
    }

    [PunRPC]
    public void BriefcaseStolen(int actorNo, float currentOwnedTime)
    {
        Debug.Log("STOLEN BY " + actorNo + " | in tutorial? : " + inTutorial);

        briefCase.transform.parent = null;
        briefCase.GetComponent<BriefCase>().ResetStolenTimer();
        ownBriefcase = false;
        briefcaseOutline.GetComponent<Renderer>().material.color = playerColours[actorNo];
        currentPlayerOwnedTime = currentOwnedTime;
        
        string logMsg = "ownBriefcase = false;";
        
        // If we stole the briefcase.
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNo)
        {
            Debug.Log("WE STOLE IT");

            stealingBriefCase = true;
            ownBriefcase = true;
            logMsg = "ownBriefcase = true;";

            if (playerTimerText != null)
                playerTimerText.GetComponent<Text>().color = Color.green;
        }
        else if (!inTutorial)
        {
            Debug.Log("WE DIDN'T STOLE IT");

            // If somebody else owns it.
            if (briefcaseOutline.GetComponent<Renderer>().material.color != Color.black)
            {
                Debug.Log("SOMEONE ELSE DID");

                playerTimerText.GetComponent<Text>().color = Color.red;

                playerTimerText.GetComponent<Text>().text = (currentPlayerOwnedTime).ToString("F0");
            }
            // If nobody owns it.
            else
            {
                Debug.Log("SIKE IT WAS NO-ONE");

                playerTimerText.GetComponent<Text>().color = Color.white;

                playerTimerText.GetComponent<Text>().text = (ownedTime).ToString("F0");
            }
        }

        Debug.Log(logMsg);
    }
}
