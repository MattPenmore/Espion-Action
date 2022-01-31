using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealBriefCase : MonoBehaviour
{
    public static float winTime = 30f;

    GameObject briefCase;

    bool ownBriefcase = false;

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
                briefCase.transform.parent.GetComponent<StealBriefCase>().ownBriefcase = false;
                briefCase.transform.parent = transform;
                briefCase.transform.position = briefCaseLocation.transform.position;
                briefCase.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}
