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

    [SerializeField] private SkinnedMeshRenderer skin;
    [SerializeField] private MeshRenderer[] grappleGun;
    [SerializeField] private GameObject playerScore;

    private Transform scoreBoard;
    private Canvas returnText;

    private GameObject gameManager;
    private GameObject playerTimerText;
    //private GameObject otherPlayerTimerText;
    private Color[] playerColours;
    private GameObject briefcaseOutline;
    private TutorialScript tutorialScript;
    private GameScript gameScript;
    float maxStealTime = 1f;
    private Transform startGameButton;

    bool stealingBriefCase;
    public static float ownedTime;
    public static float currentPlayerOwnedTime;
    public static int currentPlayerActorNo;
    public static float[] allPlayersTimes;
    float stealTimer;
    bool gameOver;
    private int numOfPlayers;

    //private string[] playerNamesOrdered;
    //private float[] playerTimesOrdered;
    //private Color[] playerColoursOrdered;
    private Coroutine scoresCo;

    [SerializeField]
    float stealDistance;

    [SerializeField]
    GameObject briefCaseLocationXZ;

    [SerializeField]
    GameObject briefCaseLocationY;

    [SerializeField]
    Animator anim;

    [SerializeField] AudioClip[] clips;
    bool playedEndSound = false;

    // Start is called before the first frame update
    void Start()
    {
        numOfPlayers = PhotonNetwork.PlayerList.Length;
        stealTimer = 0f;
        ownedTime = winTime;
        currentPlayerOwnedTime = winTime;
        gameOver = false;
        stealingBriefCase = false;
        ownBriefcase = false;
        briefCase = GameObject.FindGameObjectWithTag("BriefCase");
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        allPlayersTimes = new float[PhotonNetwork.PlayerList.Length];
        if (inTutorial)
            tutorialScript = gameManager.GetComponent<TutorialScript>();
        else
        {
            gameScript = gameManager.GetComponent<GameScript>();
            
            //playerNamesOrdered = new string[PhotonNetwork.PlayerList.Length];
            //playerTimesOrdered = new float[PhotonNetwork.PlayerList.Length];
            //playerColoursOrdered = new Color[PhotonNetwork.PlayerList.Length];
            
            scoreBoard = GameObject.FindGameObjectWithTag("ScoreBoard").transform;
            returnText = GameObject.FindGameObjectWithTag("ReturnText").GetComponent<Canvas>();
        }
        playerTimerText = GameObject.FindGameObjectWithTag("PlayerTimerText");
        //otherPlayerTimerText = GameObject.FindGameObjectWithTag("OtherPlayerTimerText");
        briefcaseOutline = GameObject.FindGameObjectWithTag("Outline");
        playerColours = new Color[] { Color.black, Color.blue, Color.red, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.grey, new Color(1f, .25f, 0f, 1f) };
        if (inTutorial && PhotonNetwork.IsMasterClient)
        {
            startGameButton = tutorialScript.startGameButton.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Break out of update loop if not the owner of this gameobject.
        if (!gameObject.GetPhotonView().IsMine)
            return;

        // Break out for the first 1s.
        if (GetComponent<PlayerController>().localTime < 1f)
        {
            return;
        }

        if (ownedTime > winTime)
            ownedTime = winTime;

        //Debug.Log(briefcaseOutline.GetComponent<Renderer>().material.color + " | " + playerColours[0]);
        if (!ownBriefcase && briefcaseOutline.GetComponent<Renderer>().material.color != playerColours[0] && !inTutorial) //black
        {
            currentPlayerOwnedTime -= Time.deltaTime;
            allPlayersTimes[currentPlayerActorNo - 1] = winTime - currentPlayerOwnedTime;
            playerTimerText.GetComponent<Text>().text = (currentPlayerOwnedTime).ToString("F0");
        }

        // Turn off interact canvas.
        if (inTutorial)
            if (tutorialScript.interactCanvas != null)
                tutorialScript.interactCanvas.enabled = false;
        else
            if (gameScript.interactCanvas != null)
                gameScript.interactCanvas.enabled = false;

        if (ownBriefcase && !gameOver)
        {
            if (playerTimerText != null)
                playerTimerText.GetComponent<Text>().text = (ownedTime).ToString("F0");
            anim.SetBool("HasBriefCase", true);
            ownedTime -= Time.deltaTime;
            allPlayersTimes[photonView.OwnerActorNr - 1] = winTime - ownedTime;
            if(ownedTime <= 0 && !inTutorial)
            {
                gameOver = true;
                Debug.Log("Win");
                gameManager.GetPhotonView().RPC("EndGame", RpcTarget.All, photonView.Owner.NickName, photonView.OwnerActorNr);
                photonView.RPC(nameof(DisplayScores), RpcTarget.AllBuffered);
            }
        }
        else if(briefCase.GetComponent<BriefCase>().stealable == true)
        {
            anim.SetBool("HasBriefCase", false);
            if (Vector3.Distance(transform.position, briefCase.transform.position) <= stealDistance)
            {
                // Turn on interact canvas.
                if (inTutorial)
                    if (tutorialScript.interactCanvas != null)
                        tutorialScript.interactCanvas.enabled = true;
                else
                    if (gameScript.interactCanvas != null)
                        gameScript.interactCanvas.enabled = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    CallBriefcaseTransfer(photonView.OwnerActorNr, true, ownedTime);
                }
            }
        }
        else
            anim.SetBool("HasBriefCase", false);

        // Turn off player renderers when in first person.
        if (firstPerson)
        {
            skin.enabled = false;
            foreach (MeshRenderer mr in grappleGun)
                mr.enabled = false;
        }
        else
        {
            skin.enabled = true;
            foreach (MeshRenderer mr in grappleGun)
                mr.enabled = true;
        }

        // Turn off briefcase renderer when in first person and own briefcase.
        if (ownBriefcase)
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

        // Check if in tutorial and if we are master client.
        if (inTutorial && PhotonNetwork.IsMasterClient)
        {
            //// Turn off interact canvas.
            //tutorialScript.interactCanvas.enabled = false;
            // Check if we are close to button.
            if (Vector3.Distance(transform.position, startGameButton.position) < 5)
            {
                LayerMask layerMask = LayerMask.GetMask("Button");
                // Raycast to see if we are looking at the button
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, 5f, layerMask, QueryTriggerInteraction.Collide))
                {
                    // Turn on interact canvas.
                    if (tutorialScript.interactCanvas != null)
                        tutorialScript.interactCanvas.enabled = true;
                    // Start game if interact pressed.
                    if (Input.GetKeyDown(KeyCode.E))
                        tutorialScript.gameObject.GetPhotonView().RPC("StartGame", RpcTarget.AllBuffered);
                }
            }
        }

        if(!inTutorial && gameScript.gameEnded && !playedEndSound)
        {
            playedEndSound = true;
            if(ownedTime < 0)
            {
                AudioSource.PlayClipAtPoint(clips[0], transform.position, GetComponent<PlayerController>().oneShotVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clips[1], transform.position, GetComponent<PlayerController>().oneShotVolume);
            }
        }
    }

    private void LateUpdate()
    {
        // Break out if not owner.
        if (!gameObject.GetPhotonView().IsMine)
            return;
        
        Vector3 briefCasePosition = new Vector3(briefCaseLocationXZ.transform.position.x, briefCaseLocationY.transform.position.y, briefCaseLocationXZ.transform.position.z);
       
        if (stealingBriefCase)
        {
            briefCase.transform.parent = null;
            stealTimer += Time.deltaTime;
            float briefcaseDistance = (briefCase.transform.position - briefCasePosition).magnitude;
            briefCase.transform.position = Vector3.Lerp(briefCase.transform.position, briefCasePosition, Mathf.Max(20, 20 * briefcaseDistance) * Time.deltaTime);
            briefCase.transform.rotation = Quaternion.Lerp(briefCase.transform.rotation, briefCaseLocationXZ.transform.localRotation, 1);

            if (Vector3.Distance(briefCase.transform.position, briefCasePosition) < 0.1f || stealTimer >= maxStealTime)
            {
                briefCase.transform.position = briefCasePosition;
                briefCase.transform.rotation = briefCaseLocationXZ.transform.localRotation;
                briefCase.transform.parent = transform;
                stealingBriefCase = false;
                stealTimer = 0f;
            }
        }
        else if (ownBriefcase && !gameOver)
        {
            briefCase.transform.parent = transform;
            briefCase.transform.position = briefCasePosition;
            briefCase.transform.localRotation = briefCaseLocationXZ.transform.localRotation;
        }
    }

    public void CallBriefcaseTransfer(int actorNo, bool transferOwner, float currentOwnedTime)
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
    public void DisplayScores()// string[] names, float[] times)
    {
        if (scoresCo != null) StopCoroutine(scoresCo);
        scoresCo = StartCoroutine(ShowPlayerScores());//names, times));
    }
    
    private IEnumerator ShowPlayerScores()//string[] names, float[] times)
    {
        yield return new WaitForSeconds(1f);
    
        for (int i = 0; i < numOfPlayers; i++)
        {
            // Spawn a score prefab for each player other than the winner.
            GameObject go = Instantiate(playerScore, scoreBoard);
            Text[] nameAndTime = go.GetComponentsInChildren<Text>();
            // Set player name and time.
            nameAndTime[0].text = PhotonNetwork.PlayerList[i].NickName;//playerNamesOrdered[i];
            nameAndTime[1].text = Mathf.Clamp(allPlayersTimes[i], 0, winTime).ToString("F0") + "s";
            // Set text colours.
            nameAndTime[0].color = playerColours[i + 1];
            nameAndTime[1].color = playerColours[i + 1];
            // Wait 1 second between each spawn.
            yield return new WaitForSeconds(1f);
        }
        // Show the "returning to lobby..." text.
        returnText.enabled = true;
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
                
                currentPlayerActorNo = actorNo;

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
