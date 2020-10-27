using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeButton : MonoBehaviour
{
    public Button btn;
    public GameObject modeName;
    public GameObject countdown;
    public GameObject lockIcon;
    public Text countdownText;
    public bool canClick = false;
    public GameObject buttonBorder;
    public GameObject glow;

    public Color enabledColor = new Color(97, 98, 99);
    public Color disabledColor = new Color(60, 60, 60);

    public void EnableGameMode()
    {
        lockIcon.SetActive(false);
        modeName.SetActive(true);
        modeName.GetComponent<Text>().color = enabledColor;
        buttonBorder.GetComponent<Image>().color = enabledColor;

        if (!GameController.script.unlockedChaos && !GameController.script.unlockedImpossible && !GameController.script.unlockedUnfair)
            glow.SetActive(true);
        else
            glow.SetActive(false);
        countdown.SetActive(false);

        canClick = true;
    }

    public void ShowCountdown()
    {
        lockIcon.SetActive(true);
        modeName.SetActive(false);
        modeName.GetComponent<Text>().color = disabledColor;
        buttonBorder.GetComponent<Image>().color = disabledColor;
        glow.SetActive(false);
        countdown.SetActive(true);

        canClick = false;
    }

    public void DisableGameMode()
    {
        lockIcon.SetActive(true);
        modeName.SetActive(true);
        modeName.GetComponent<Text>().color = disabledColor;
        buttonBorder.GetComponent<Image>().color = disabledColor;
        glow.SetActive(false);
        countdown.SetActive(false);

        canClick = false;
    }

    public void SetCountdownText(string ctdown)
    {
        ShowCountdown();
        countdownText.text = ctdown;
    }
}
