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

    // Update is called once per frame
    void Update()
    {
        if(hasUpgrade)
        {
            timeHadUpgrade += Time.deltaTime;
            if(timeHadUpgrade >= upgradeTime)
            {
                hasUpgrade = false;
                timeHadUpgrade = 0;
            }

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
                    obj.layer = 9;
                }
                foreach (GameObject mat in playerMaterials)
                {
                    Material material = mat.GetComponent<Renderer>().material;

                    Color newColor = new Color32((byte)(material.color.r * 255), (byte)(material.color.g * 255), (byte)(material.color.b * 255), 255);

                    material.color = newColor;
                }
            }

        }
        else
        {
            timeHadUpgrade = 0;
            foreach (GameObject obj in playerColliders)
            {
                obj.layer = 9;
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
