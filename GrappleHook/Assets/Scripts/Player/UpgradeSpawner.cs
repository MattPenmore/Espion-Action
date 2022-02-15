using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject[] upgrades;

    [SerializeField]
    GameObject spawnLocation;

    [SerializeField]
    GameObject currentUpgrade;

    [SerializeField]
    float upgradeSpawnTime;

    [SerializeField]
    float timetoSpawn;

    public bool hasUpgradeSpawned = false;



    // Start is called before the first frame update
    void Start()
    {
        timetoSpawn = upgradeSpawnTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(hasUpgradeSpawned == false && timetoSpawn > 0)
        {
            timetoSpawn -= Time.deltaTime;
        }

        if(hasUpgradeSpawned == false && timetoSpawn <= 0)
        {
            int rand = Random.Range(0, upgrades.Length);

            currentUpgrade = upgrades[rand];

            GameObject spawn = Instantiate(currentUpgrade, spawnLocation.transform.position, spawnLocation.transform.rotation);
            spawn.transform.parent = gameObject.transform;
            hasUpgradeSpawned = true;
            timetoSpawn = upgradeSpawnTime;
        }
    }
}
