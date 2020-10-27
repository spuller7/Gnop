using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CloudOnce;

public class CloudOnceServices : MonoBehaviour
{
    public static CloudOnceServices instance;

    private void Awake()
    {
        TestSingleon();
    }

    private void TestSingleon()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //Submits score to global leaderboards
    public void SubmitScore(int score)
    {
        Leaderboards.HighScore.SubmitScore(score);
    }

    public bool IsConnected()
    {
        return Cloud.IsSignedIn;
    }
}
