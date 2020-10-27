using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class TitleBall : MonoBehaviour
{

    [SerializeField]
    float speed;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip[] hitSoundEffects;

    [SerializeField]
    private GameObject letterG;
    [SerializeField]
    private GameObject letterN;
    [SerializeField]
    private GameObject letterP;

    private int lastAudioClip;
    Vector2 direction;

    void Start()
    {
        direction = Vector2.one.normalized;

        int dir = Random.Range(1, 3);
        if (dir == 1)
        {
            direction = -direction;
        }
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Title Collider")
        {
                direction.y = -direction.y;
        }
        else
        {
            direction.x = -direction.x;

            if(collision.name == "Left Collider")
            {
                letterG.GetComponent<SpriteRenderer>().color = this.gameObject.GetComponent<SpriteRenderer>().color;
                letterN.GetComponent<SpriteRenderer>().color = this.gameObject.GetComponent<SpriteRenderer>().color;
            }
            else
            {
                letterP.GetComponent<SpriteRenderer>().color = this.gameObject.GetComponent<SpriteRenderer>().color;
            }
        }
    }

    private void PlayCollisionSound()
    {
        int numberOfSounds = hitSoundEffects.Length;

        int chosenSound = Random.Range(0, numberOfSounds);

        if (chosenSound == lastAudioClip)
        {
            if (chosenSound != 0)
            {
                chosenSound--;
            }
            else
            {
                chosenSound++;
            }
        }

        this.audioSource.clip = hitSoundEffects[chosenSound];

        lastAudioClip = chosenSound;
        this.audioSource.Play();
    }
}
