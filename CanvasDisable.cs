using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasDisable : MonoBehaviour {

    [SerializeField]
    public GameObject gameOver;

    [SerializeField]
    public GameManager gm;

    public void DisableButtons()
    {
        gameOver.SetActive(false);
        gm.ContinueGame();
    }
}
