using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    [SerializeField]
    private GameObject deathParticles;

    private int spawnedScore = 0;

    public void init(int score)
    {
        spawnedScore = score;
    }

    public void OnDestroy()
    {
        GameObject part = Instantiate(deathParticles);
        part.transform.position = this.gameObject.transform.position;
    }

    public int GetSpawnedScore()
    {
        return spawnedScore;
    }
}
