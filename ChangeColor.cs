using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour {

    [SerializeField]
    private Color[] colors;
    public bool isMenu = false;
    private int currentColor = 0;
    private int nextColor = 1;
    public TrailRenderer trailRenderer;
    float t = 0;

    private void Awake()
    {
        if (!isMenu)
        {
            colors = GameObject.Find("Game Manager").GetComponent<GameManager>().getTheme().colorCycle;
        }
    }

    void Update () {

        if(t >= 1)
        {
            currentColor = nextColor;
            nextColor++;

            if (nextColor == colors.Length)
            {
                nextColor = 0;
            }

            t = 0;
        }
        else
        {
            t += Time.deltaTime/4;
            Color color = Color.Lerp(colors[currentColor], colors[nextColor], t);
            gameObject.GetComponent<SpriteRenderer>().color = color;
            trailRenderer.startColor = color;
        }
    }

    private int GetNextColor()
    {
        int theNextColor = nextColor++;
        if(theNextColor == colors.Length)
        {
            theNextColor = 0;
        }

        return theNextColor;
    }
}
