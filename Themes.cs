using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Themes : MonoBehaviour {

    public enum ThemeType {
        Light, Dark
    };


    public ThemeType type;

    public Color backgroundColor;
    public Color bestColor;
    public Color newHighScoreColor;
    public Color[] colorCycle;
}
