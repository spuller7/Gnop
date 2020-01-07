using UnityEngine;

public class SoundManager : MonoBehaviour {

    private AudioSource audioSource;

    [SerializeField]
    public AudioClip ballSpawn;

    [SerializeField]
    public AudioClip highScoreSound;

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayBallSpawn()
    {
        audioSource.clip = ballSpawn;
        audioSource.Play();
    }

    public void PlayHighScore()
    {
        audioSource.clip = highScoreSound;
        audioSource.Play();
    }
}
