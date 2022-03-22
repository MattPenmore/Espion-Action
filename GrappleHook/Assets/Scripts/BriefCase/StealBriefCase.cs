using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealBriefCase : MonoBehaviour
{
    public static float winTime = 30f;

    public GameObject briefCase;

    public bool ownBriefcase = false;
    bool stealingBriefCase = false;
    float ownedTime = 0;

    [SerializeField]
    float stealDistance;

    [SerializeField]
    GameObject briefCaseLocation;

    // Start is called before the first frame update
    void Start()
    {
        briefCase = GameObject.FindGameObjectWithTag("BriefCase");
    }

    // Update is called once per frame
    void Update()
    {
        if(ownBriefcase)
        {
            ownedTime += Time.deltaTime;
            if(ownedTime >= winTime)
            {
                Debug.Log("Win");
            }
        }
        else if(briefCase.GetComponent<BriefCase>().stealable == true)
        {
            if(Vector3.Distance(transform.position, briefCase.transform.position) <= stealDistance && Input.GetKeyDown(KeyCode.Return))
            {
                if(briefCase.transform.parent)
                {
                    if(briefCase.transform.parent.tag == "Player")
                    {
                        briefCase.transform.parent.GetComponent<StealBriefCase>().ownBriefcase = false;
                    }
                }
                briefCase.transform.parent = null;
                stealingBriefCase = true;
                ownBriefcase = true;

                
            }
        }

        if(stealingBriefCase)
        {
            briefCase.transform.position = Vector3.Lerp(briefCase.transform.position, briefCaseLocation.transform.position, 1 * Time.deltaTime);
            briefCase.transform.rotation = Quaternion.Lerp(briefCase.transform.rotation, briefCaseLocation.transform.rotation, 1);

            if(Vector3.Distance(briefCase.transform.position, briefCaseLocation.transform.position) > 0.05f)
            {
                briefCase.transform.position = briefCaseLocation.transform.position;
                briefCase.transform.rotation = briefCaseLocation.transform.rotation;
                briefCase.transform.parent = transform;
                stealingBriefCase = false;
            }
        }
    }
}
