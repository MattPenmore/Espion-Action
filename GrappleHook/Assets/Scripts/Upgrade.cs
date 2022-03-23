using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    [SerializeField]
    string upgradeName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.GetComponent<PlayerUpgrades>())
        {
            if (!other.transform.parent.GetComponent<PlayerUpgrades>().hasUpgrade)
            {
                other.transform.parent.GetComponent<PlayerUpgrades>().currentUpgrade = upgradeName;
                other.transform.parent.GetComponent<PlayerUpgrades>().hasUpgrade = true;
                transform.parent.GetComponent<UpgradeSpawner>().hasUpgradeSpawned = false;
                Destroy(gameObject);
            }
        }
        else if(other.transform.GetComponent<PlayerUpgrades>())
        {
            other.transform.GetComponent<PlayerUpgrades>().currentUpgrade = upgradeName;
            other.transform.GetComponent<PlayerUpgrades>().hasUpgrade = true;
            transform.parent.GetComponent<UpgradeSpawner>().hasUpgradeSpawned = false;
            Destroy(gameObject);
        }
    }
}
