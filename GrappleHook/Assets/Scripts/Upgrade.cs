using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade : MonoBehaviour
{
    [SerializeField]
    string upgradeName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.GetComponent<PlayerUpgrades>())
        {
            if (!other.transform.root.GetComponent<PlayerUpgrades>().hasUpgrade)
            {
                other.transform.root.GetComponent<PlayerUpgrades>().currentUpgrade = upgradeName;
                other.transform.root.GetComponent<PlayerUpgrades>().hasUpgrade = true;
                transform.parent.GetComponent<UpgradeSpawner>().hasUpgradeSpawned = false;
                Destroy(gameObject);
            }
        }
    }
}
