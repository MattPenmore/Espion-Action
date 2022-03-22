using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BriefCase : MonoBehaviour
{
    [SerializeField]
    float immunityTime;

    float timeSinceStolen;

    public bool stealable = true;


    public Vector3 startingPosition = new Vector3();
    public Quaternion startingRotation = new Quaternion();

    private void Start()
    {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceStolen += Time.deltaTime;

        if(timeSinceStolen > immunityTime)
        {
            stealable = true;
        }
        else
        {
            stealable = false;
        }
    }
}
