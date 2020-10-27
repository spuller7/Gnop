using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeManager : MonoBehaviour
{
    [SerializeField]
    private GameObject chaosButton;

    [SerializeField]
    private GameObject impossibleButton;

    [SerializeField]
    private GameObject unfairButton;

    [SerializeField]
    private GameObject popup;

    [SerializeField]
    private Animator anim;

    [SerializeField]
    private Text popupText;
 
    private GameController.GameMode popupGameMode = GameController.GameMode.Classic;

    private ulong chaosTimer;
    private ulong impossibleTimer;
    private ulong unfairTimer;

    private bool trialChaos = false;
    private bool trialImpossible = false;
    private bool trialUnfair = false;

    private float timeDelay = 86400000.0f;

    private void Start()
    {
        //PlayerPrefs.DeleteAll();

        popup.SetActive(false);
        int hasPlayedAll = PlayerPrefs.GetInt("HasPlayedAll", 0);

        trialChaos = PlayerPrefs.GetInt("TrialChaos", 0) != 0;
        trialImpossible = PlayerPrefs.GetInt("TrialImpossible", 0) != 0;
        trialUnfair = PlayerPrefs.GetInt("TrialUnfair", 0) != 0;

        chaosTimer = ulong.Parse(PlayerPrefs.GetString("ChaosTimer", DateTime.Now.AddSeconds((timeDelay / 1000f) * -2).Ticks.ToString()));
        impossibleTimer = ulong.Parse(PlayerPrefs.GetString("ImpossibleTimer", DateTime.Now.AddSeconds((timeDelay / 1000f) * -1).Ticks.ToString()));
        unfairTimer = ulong.Parse(PlayerPrefs.GetString("UnfairTimer", DateTime.Now.Ticks.ToString()));
        PlayerPrefs.SetString("ChaosTimer", chaosTimer.ToString());
        PlayerPrefs.SetString("ImpossibleTimer", impossibleTimer.ToString());
        PlayerPrefs.SetString("UnfairTimer", unfairTimer.ToString());


        UpdateGameModeDisplay();
    }

    private void Update()
    {
        List<ulong> timers = new List<ulong> { chaosTimer, impossibleTimer, unfairTimer };

        int idx = 0;
        foreach (ulong timer in timers)
        {
            float secondsLeft = TimeRemaining(timer);

            if (secondsLeft < 0)
            {
                if (idx == 0)
                {
                    trialChaos = !trialChaos;
                    PlayerPrefs.SetInt("TrialChaos", trialChaos ? 1 : 0);

                    if (trialChaos)
                        PlayerPrefs.SetString("ChaosTimer", DateTime.Now.AddSeconds((timeDelay / 1000f) * -2).Ticks.ToString());
                    else
                        PlayerPrefs.SetString("ChaosTimer", DateTime.Now.Ticks.ToString());
                }   
                else if (idx == 1)
                {
                    trialImpossible = !trialImpossible;
                    PlayerPrefs.SetInt("TrialImpossible", trialImpossible ? 1 : 0);

                    if (trialImpossible)
                        PlayerPrefs.SetString("ImpossibleTimer", DateTime.Now.AddSeconds((timeDelay / 1000f) * -2).Ticks.ToString());
                    else
                        PlayerPrefs.SetString("ImpossibleTimer", DateTime.Now.Ticks.ToString());
                }
                else if (idx == 2)
                {
                    trialUnfair = !trialUnfair;
                    PlayerPrefs.SetInt("TrialUnfair", trialUnfair ? 1 : 0);

                    if (trialUnfair)
                        PlayerPrefs.SetString("UnfairTimer", DateTime.Now.AddSeconds((timeDelay / 1000f) * -2).Ticks.ToString());
                    else
                        PlayerPrefs.SetString("UnfairTimer", DateTime.Now.Ticks.ToString());
                }

                chaosTimer = ulong.Parse(PlayerPrefs.GetString("ChaosTimer", DateTime.Now.AddSeconds((timeDelay / 1000f) * -2).Ticks.ToString()));
                impossibleTimer = ulong.Parse(PlayerPrefs.GetString("ImpossibleTimer", DateTime.Now.AddSeconds((timeDelay / 1000f) * -1).Ticks.ToString()));
                unfairTimer = ulong.Parse(PlayerPrefs.GetString("UnfairTimer", DateTime.Now.Ticks.ToString()));

                UpdateGameModeDisplay();
            }
            else if (secondsLeft < (timeDelay / 1000f))
            {
                string timeText = "";

                timeText += ((int)secondsLeft / 3600).ToString() + "h ";
                secondsLeft -= ((int)secondsLeft / 3600) * 3600;
                timeText += ((int)secondsLeft / 60).ToString("00") + "m ";
                timeText += (secondsLeft % 60).ToString("00") + "s";

                if (idx == 0 && !GameController.script.unlockedChaos && !trialChaos)
                    chaosButton.GetComponent<GameModeButton>().SetCountdownText(timeText);
                else if (idx == 1 && !GameController.script.unlockedImpossible && !trialImpossible)
                    impossibleButton.GetComponent<GameModeButton>().SetCountdownText(timeText);
                else if (idx == 2 && !GameController.script.unlockedUnfair && !trialUnfair)
                    unfairButton.GetComponent<GameModeButton>().SetCountdownText(timeText);
            }

            idx++;
        }
    }

    // returns in seconds
    private float TimeRemaining(ulong timer)
    {
        ulong diff = ((ulong)DateTime.Now.Ticks - timer);
        ulong m = diff / TimeSpan.TicksPerMillisecond;
        
        float secondsLeft = (float)((timeDelay * 3) - m) / 1000.0f;

        return secondsLeft;
    }

    public void PlayClassic()
    {
        GameController.script.PlayGame(GameController.GameMode.Classic);
    }

    public void PlayChaos()
    {
        if (chaosButton.GetComponent<GameModeButton>().canClick)
        {
            GameController.script.PlayGame(GameController.GameMode.Chaos);
        }
        else
        {
            popupGameMode = GameController.GameMode.Chaos;
            popupText.text = "Unlock Chaos Mode permanently for just $0.99?";
            popup.SetActive(true);
            anim.SetBool("show", true);
        }
    }

    public void PlayImpossible()
    {
        if (impossibleButton.GetComponent<GameModeButton>().canClick)
        {
            GameController.script.PlayGame(GameController.GameMode.Impossible);
        }
        else
        {
            popupGameMode = GameController.GameMode.Impossible;
            popupText.text = "Unlock Impossible Mode permanently for just $0.99?";
            popup.SetActive(true);
            anim.SetBool("show", true);
        }
    }

    public void PlayUnfair()
    {
        if (unfairButton.GetComponent<GameModeButton>().canClick)
        {
            GameController.script.PlayGame(GameController.GameMode.Unfair);
        }
        else
        {
            popupGameMode = GameController.GameMode.Unfair;
            popupText.text = "Unlock Unfair Mode permanently for just $0.99?";
            popup.SetActive(true);
            anim.SetBool("show", true);
        }
    }

    public void ClosePopup()
    {
        anim.SetBool("show", false);
        StartCoroutine(DisabledPopup());
    }

    private IEnumerator DisabledPopup()
    {
        yield return new WaitForSeconds(1f);
        popup.SetActive(false);
    }

    public void UpdateGameModeDisplay()
    {
        ClosePopup();

        // Enable Playable Gamemodes
        if (GameController.script.unlockedChaos || trialChaos)
            chaosButton.GetComponent<GameModeButton>().EnableGameMode();
        else
            chaosButton.GetComponent<GameModeButton>().DisableGameMode();

        if (GameController.script.unlockedImpossible || trialImpossible)
            impossibleButton.GetComponent<GameModeButton>().EnableGameMode();
        else
            impossibleButton.GetComponent<GameModeButton>().DisableGameMode();

        if (GameController.script.unlockedUnfair || trialUnfair)
            unfairButton.GetComponent<GameModeButton>().EnableGameMode();
        else
            unfairButton.GetComponent<GameModeButton>().DisableGameMode();
    }

    public void BuyGameMode()
    {
        GameController.script.BuyGameMode(popupGameMode);
    }

    public void TestPurchase()
    {
        GameController.script.TestPurchase();
    }
}
