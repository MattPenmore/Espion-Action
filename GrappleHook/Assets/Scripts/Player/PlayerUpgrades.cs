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
        }
        else
        {
            timeHadUpgrade = 0;
        }
    }
}
