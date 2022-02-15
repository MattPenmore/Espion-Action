using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealBriefCase : MonoBehaviour
{
    public static float winTime = 30f;

    GameObject briefCase;

    public bool ownBriefcase = false;
    bool stealingBriefCase = false;
    float ownedTime = 0;

    [SerializeField]
    float stealDistance;

    [SerializeField]
    GameObject briefCaseLocation;

    [SerializeField]
    GameObject hook;

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
                briefCase.transform.parent.GetComponent<StealBriefCase>().ownBriefcase = false;
                briefCase.transform.parent = transform;
                ownBriefcase = true;
                stealingBriefCase = true;

                
            }
        }

        if(stealingBriefCase)
        {
            briefCase.transform.position = Vector3.Lerp(briefCase.transform.position, briefCaseLocation.transform.position, 1);
            briefCase.transform.rotation = Quaternion.Lerp(briefCase.transform.rotation, briefCaseLocation.transform.rotation, 1);

            if(Vector3.Distance(briefCase.transform.position, briefCaseLocation.transform.position) > 0.05f)
            {
                briefCase.transform.position = briefCaseLocation.transform.position;
                briefCase.transform.rotation = briefCaseLocation.transform.rotation;
                stealingBriefCase = false;
            }
        }


        //Steal with grappling hook
        if(briefCase.transform.parent == hook.transform)
        {
            if (Vector3.Distance(transform.position, briefCase.transform.position) <= stealDistance)
            {
                //briefCase.transform.parent.GetComponent<StealBriefCase>().ownBriefcase = false;
                briefCase.transform.parent = transform;
                ownBriefcase = true;
                stealingBriefCase = true;
            }
        }
    }
}
