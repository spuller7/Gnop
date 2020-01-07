using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour {

    public int seconds = 2;

	void Start () {
        StartCoroutine(Destroy());
	}
	
	private IEnumerator Destroy()
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
