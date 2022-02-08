using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBoost : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerUpgrades>())
        {
            if (!other.GetComponent<PlayerUpgrades>().hasUpgrade)
            {
                other.GetComponent<PlayerUpgrades>().currentUpgrade = "JumpBoost";
                other.GetComponent<PlayerUpgrades>().hasUpgrade = true;
                transform.parent.GetComponent<UpgradeSpawner>().hasUpgradeSpawned = false;
                Destroy(gameObject);
            }
        }

        if (other.transform.root.GetComponent<PlayerUpgrades>())
        {
            if (!other.transform.root.GetComponent<PlayerUpgrades>().hasUpgrade)
            {
                other.transform.root.GetComponent<PlayerUpgrades>().currentUpgrade = "JumpBoost";
                other.transform.root.GetComponent<PlayerUpgrades>().hasUpgrade = true;
                transform.parent.GetComponent<UpgradeSpawner>().hasUpgradeSpawned = false;
                Destroy(gameObject);
            }
        }
    }
}
