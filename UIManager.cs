using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

    [SerializeField]
    public SoundManager soundManager;

    [SerializeField]
    public GameManager gameManager;

    [SerializeField]
    public GameObject gameOver;

    [SerializeField]
    public Text score;

    [SerializeField]
    public Text highScore;

    [SerializeField]
    public GameObject newHighScore;

    [SerializeField]
    public GameObject bestScoreLabel;

    [Header("Animators")]

    [SerializeField]
    public Animator canvasAnimator;

    [SerializeField]
    public Animator scoreAnimator;

    [SerializeField]
    private GameObject canContinueButton;

    [SerializeField]
    private Text continueText;

    [SerializeField]
    private Button noAdsButton;

    [SerializeField]
    private Text bestText;

    [SerializeField]
    private Text noAdsText;

    [SerializeField]
    private Button leaderboardButton;

    [SerializeField]
    private Text leaderboardsText;

    [SerializeField]
    private Color activeText;

    [SerializeField]
    private Color inactiveText;

    [SerializeField]
    private Color lightThemeTextColor;

    [SerializeField]
    private Color darkThemeTextColor;

    [Header("Background Textures")]
    public Texture[] textures;

    private int currentTexture = 3;

    [Space(10)]

    [SerializeField]
    private ParticleSystem backgroundEmiiter;

    [Space(10)]

    [SerializeField]
    private GameObject[] cornerShapes;

    [SerializeField]
    private GameObject safeZoneBuffers;

    [SerializeField]
    private GameObject tapToStart;

    [SerializeField]
    private Text tapToStartText;

    private int bestScoreVal;

    private void Awake()
    {
        if (gameManager.getTheme().type != Themes.ThemeType.Dark)
        {
            score.color = lightThemeTextColor;
        }
        else
        {
            score.color = darkThemeTextColor;
        }

        bestText.color = gameManager.getTheme().bestColor;
    }

    void Start ()
    {
        gameOver.SetActive(false);
        newHighScore.SetActive(false);
        tapToStart.SetActive(true);

        bestScoreVal = GameController.script.GetHighScore();
        highScore.text = bestScoreVal.ToString();
    }

    public void ChangeScore(int score)
    {
        this.score.text = score.ToString();
    }

    public void DisplayGameOver(int score)
    {
        if (GameController.script.CanContinue(score))
        {
            canContinueButton.SetActive(true);
        }
        else
        {
            canContinueButton.SetActive(false);
        }

        if (GameController.script.noAds)
        {
            noAdsButton.interactable = false;
            noAdsText.enabled = false;
        }
        else
        {
            noAdsButton.interactable = true;
            noAdsText.color = activeText;
        }

        if (CloudOnceServices.instance.IsConnected())
        {
            leaderboardButton.interactable = true;
            leaderboardsText.text = "Leaderboards";
            leaderboardsText.color = activeText;
        }
        else
        {
            leaderboardButton.interactable = false;
            leaderboardsText.text = "Sign In Required";
            leaderboardsText.color = inactiveText;
        }

        gameOver.SetActive(true);
        canvasAnimator.SetBool("isPlaying", false);
        scoreAnimator.SetBool("isPlaying", false);

        UpdateHighScore(score);
    }

    public void ContinueGame()
    {
        tapToStartText.text = "Tap To Continue";
        canContinueButton.SetActive(false);
        canvasAnimator.SetBool("isPlaying", true);
        scoreAnimator.SetBool("isContinue", true);
    }

    private void UpdateHighScore(int score)
    {
        if(score > bestScoreVal)
        {
            GameController.script.UpdateHighScore(score);
            highScore.text = score.ToString();
            bestScoreLabel.SetActive(false);
            highScore.gameObject.SetActive(false);
            newHighScore.SetActive(true);
            soundManager.PlayHighScore();
        }
    }

    public void FadeOutScore()
    {
        scoreAnimator.SetBool("isVisible", false);
    }

    public void FadeInScore()
    {
        tapToStart.SetActive(false);
        scoreAnimator.SetBool("isContinue", false);
        scoreAnimator.SetBool("isVisible", true);
        ChangeBackgroundTexture();
        scoreAnimator.SetBool("isPlaying", true);
    }

    public void ChangeBackgroundTexture()
    {
        currentTexture = (currentTexture + 1) % textures.Length;

        Material material = backgroundEmiiter.GetComponent<ParticleSystemRenderer>().material;
        material.mainTexture = textures[currentTexture];
    }
}
