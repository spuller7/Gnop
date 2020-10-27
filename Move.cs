using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public GameManager gm;
    
    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * gm.GetSpeed() * gm.GetPadddleSpeedMultiplier());
    }
}
