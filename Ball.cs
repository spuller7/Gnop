using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class Ball : MonoBehaviour {

    [SerializeField]
    private float speed;

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
    private bool isFake = false;
    private bool isPlaying = true;

    private CameraShake camShake;

    void Start ()
    {
        camShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
        float angle = Random.Range(Screen.height, Screen.height / 5 + Screen.height / 2);

        direction = Vector3.Normalize(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, angle, 0)) - transform.position);
        direction = -direction.normalized;

        if (GameController.script.GetGameMode() != GameController.GameMode.Unfair)
        {
            int dir = Random.Range(1, 3);
            if (dir == 1)
            {
                direction = -direction;
            }
        }
    }
	
	void Update ()
    {
        speed = gameManager.GetSpeed();
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

    private Vector2 left = new Vector2(1f, 0f);
    private Vector2 right = new Vector2(-1f, 0f);
    private Vector2 top = new Vector2(0f, -1f);
    private Vector2 bottom = new Vector2(0f, 1f);
    private float wt = 0.15f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Paddle")
        {
            Vector2 paddleCollision = collision.GetContact(0).normal;
            direction = Vector2.Reflect(direction, paddleCollision);
            direction = direction.normalized;

            if (GameController.script.GetGameMode() == GameController.GameMode.Impossible)
            {

            }

            wt = Random.Range(0.11f, 0.15f);

            int paddleDirection = gameManager.GetDirection();
            // Paddle is moving counter clockwise
            if (paddleDirection == 1)
            {
                if (paddleCollision == left)
                    direction = new Vector2(direction.x, direction.y - ((direction.y + 1) * wt));
                else if (paddleCollision == bottom)
                    direction = new Vector2(direction.x + ((1 - direction.x) * wt), direction.y);
                else if (paddleCollision == right)
                    direction = new Vector2(direction.x, direction.y + ((1 - direction.y) * wt));
                else if (paddleCollision == top)
                    direction = new Vector2(direction.x - ((direction.x + 1) * wt), direction.y);
            }
            // Paddle is moving clockwise
            else if (paddleDirection == -1)
            {
                if (paddleCollision == left)
                    direction = new Vector2(direction.x, direction.y + ((1 - direction.y) * wt));
                else if (paddleCollision == bottom)
                    direction = new Vector2(direction.x - ((direction.x + 1) * wt), direction.y);
                else if (paddleCollision == right)
                    direction = new Vector2(direction.x, direction.y - ((direction.y + 1) * wt));
                else if (paddleCollision == top)
                    direction = new Vector2(direction.x + ((1 - direction.x) * wt), direction.y);
            }

            direction = direction.normalized;
            
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Add Ball")
        {
            gameManager.HitPowerup(collision.gameObject.GetComponent<Powerup>());
            Destroy(collision.gameObject);
        }
    }

    public void Init(GameManager gameManager, bool isFake = false)
    {
        this.isFake = isFake;
        this.gameManager = gameManager;
    }

    private IEnumerator CallGameOver()
    {
        yield return new WaitForSeconds(0.75f);
        gameManager.DeleteBall(gameObject, isFake);
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
