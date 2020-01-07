using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class Ball : MonoBehaviour {

    [SerializeField]
    float speed;

    [SerializeField]
    private GameObject paddleHitParticles;

    [SerializeField]
    private GameObject circleHitParticles;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip[] hitSoundEffects;

    private int lastAudioClip;

    Vector2 direction;
    private GameManager gameManager;
    private bool isPlaying = true;

    private CameraShake camShake;

    void Start ()
    {
        camShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        float angle = Random.Range(Screen.height, Screen.height / 5 + Screen.height / 2);

        direction = Vector3.Normalize(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, angle, 0)) - transform.position);
        direction = direction.normalized;

        int dir = Random.Range(1, 3);
        if (dir == 1)
        {
            direction = -direction;
        }
    }
	
	void Update ()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        if (isPlaying)
        {

            if (gameObject.transform.position.y > GameManager.topRight.y ||
                gameObject.transform.position.y<GameManager.bottomLeft.y ||
                gameObject.transform.position.x> GameManager.topRight.x ||
                gameObject.transform.position.x<GameManager.bottomLeft.x)
            {
                isPlaying = false;
                StartCoroutine(CallGameOver());
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Paddle")
        {
            direction = Vector2.Reflect(direction, collision.GetContact(0).normal);

            Color curColor = this.gameObject.GetComponent<SpriteRenderer>().color;
            collision.gameObject.GetComponent<LineRenderer>().startColor = curColor;
            collision.gameObject.GetComponent<LineRenderer>().endColor = curColor;

            if (gameManager.score < 0)
            {
                gameManager.score = 0;
            }

            GameObject part = Instantiate(paddleHitParticles);
            part.transform.position = this.gameObject.transform.position;
            Quaternion toRotation = Quaternion.FromToRotation(part.transform.up, direction);
            part.transform.rotation = toRotation;

            camShake.shakeDuration = 0.35f;

            var partSystem = part.GetComponent<ParticleSystem>().main;
            partSystem.startColor = curColor;
            PlayCollisionSound();
            gameManager.EarnPoint();
        }
        else if (collision.gameObject.tag == "Middle Line")
        {
            direction = Vector2.Reflect(direction, collision.GetContact(0).normal); 

            Color curColor = this.gameObject.GetComponent<SpriteRenderer>().color;

            collision.gameObject.GetComponent<LineRenderer>().startColor = curColor;
            collision.gameObject.GetComponent<LineRenderer>().endColor = curColor;

            GameObject part = Instantiate(circleHitParticles);
            part.transform.position = this.gameObject.transform.position;
            var col = part.GetComponent<ParticleSystem>().colorOverLifetime;
            col.color = curColor;
            gameManager.EarnPoint();
        }
    }

    public void AssignGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    private IEnumerator CallGameOver()
    {
        yield return new WaitForSeconds(0.75f);
        gameManager.DeleteBall(gameObject);
    }

    private void PlayCollisionSound()
    {
        int numberOfSounds = hitSoundEffects.Length;

        int chosenSound = Random.Range(0, numberOfSounds);

        if (chosenSound == lastAudioClip)
        {
            if(chosenSound != 0)
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
