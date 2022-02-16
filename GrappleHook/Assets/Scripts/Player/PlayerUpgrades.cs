using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    public bool hasUpgrade;

    public string currentUpgrade;

    public float jumpUpgradeValue;
    public float speedUpgradeValue;

    [SerializeField]
    float upgradeTime;

    [SerializeField]
    GameObject[] playerColliders;

    [SerializeField]
    GameObject[] playerMaterials;

    float timeHadUpgrade = 0;


    Vector3 rewindPosition;
    Quaternion rewindRotation;
    Quaternion rewindLookRotation;

    [SerializeField]
    Transform look;

    [SerializeField]
    float rewindDistance;

    [SerializeField]
    Rigidbody rb;

    // Update is called once per frame
    void Update()
    {
        if(hasUpgrade)
        {

            if(currentUpgrade == "Wraith")
            {
                foreach(GameObject obj in playerColliders)
                {
                    obj.layer = 6;
                }
                foreach (GameObject mat in playerMaterials)
                {
                    Material material = mat.GetComponent<Renderer>().material;

                    

                    Color newColor = new Color32((byte)(material.color.r * 255), (byte)(material.color.g * 255), (byte)(material.color.b * 255), 100);

                    material.color = newColor;
                }
            }
            else
            {
                foreach (GameObject obj in playerColliders)
                {
                    obj.layer = 1;
                }
                foreach (GameObject mat in playerMaterials)
                {
                    Material material = mat.GetComponent<Renderer>().material;

                    Color newColor = new Color32((byte)(material.color.r * 255), (byte)(material.color.g * 255), (byte)(material.color.b * 255), 255);

                    material.color = newColor;
                }
            }

            if(currentUpgrade == "TimeRewind")
            {
                if (timeHadUpgrade == 0)
                {
                    rewindPosition = transform.position;
                    rewindRotation = transform.rotation;
                    rewindLookRotation = look.rotation;
                }

                if(Vector3.Distance(transform.position, rewindPosition) > rewindDistance || Input.GetKeyDown(KeyCode.R))
                {
                    transform.position = rewindPosition;
                    transform.rotation = rewindRotation;
                    look.rotation = rewindLookRotation;
                    rb.velocity = Vector3.zero;
                    hasUpgrade = false;
                    timeHadUpgrade = 0;
                }
            }

            timeHadUpgrade += Time.deltaTime;
            if(timeHadUpgrade >= upgradeTime)
            {
                hasUpgrade = false;
                timeHadUpgrade = 0;
            }
        }
        else
        {
            timeHadUpgrade = 0;
            foreach (GameObject obj in playerColliders)
            {
                obj.layer = 1;
            }
            foreach (GameObject mat in playerMaterials)
            {
                Material material = mat.GetComponent<Renderer>().material;

                Color newColor = new Color32((byte)(material.color.r * 255), (byte)(material.color.g * 255), (byte)(material.color.b * 255), 255);

                material.color = newColor;
            }
        }
    }
}
