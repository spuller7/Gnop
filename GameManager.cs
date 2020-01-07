using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    public UIManager uiManager;

    [SerializeField]
    public SoundManager soundManager;

    [SerializeField]
    public PaddleController pc;

    [SerializeField]
    private MiddleLineRenderer mlr;

    [Header("Color Themes")]
    public Themes[] themes;

    private int selectedTheme;

    [Header("Misc")]

    [SerializeField]
    private GameObject circleHitParticles;

    [SerializeField]
    private float paddledWidth = 0.2f;

    [SerializeField]
    private GameObject[] screenCorners = new GameObject[8];

    public int score = 0;
    public Ball ball;
    public Paddle paddle;
    private int ballCount;
    private int paddlesReady = 0;
    private int continueReady = 0;
    private int numHitCircle = 0;

    public static Vector2 bottomLeft;
    public static Vector2 topRight;

    List<Paddle> paddles = new List<Paddle>();
    public Vector3[] pathVertices = new Vector3[8];

    private bool isPlaying = false;
    private bool waitingToContinueGame = false;
    public bool waitingForAd = false;

    private void Awake()
    {
        //selectedTheme = Random.Range(0, themes.Length);
        selectedTheme = 0;

        Camera.main.backgroundColor = themes[selectedTheme].backgroundColor;

        //Left Side - Top
        pathVertices[0] = Camera.main.ScreenToWorldPoint(screenCorners[0].transform.position);
        pathVertices[0] = new Vector3(pathVertices[0].x + paddledWidth, pathVertices[0].y - paddledWidth, 0);

        //Left Side - Bottom
        pathVertices[1] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[1].transform.position.x, screenCorners[1].transform.position.y, 0));
        pathVertices[1] = new Vector3(pathVertices[1].x + paddledWidth, pathVertices[1].y + paddledWidth, 0);

        //Bottom Side - Left
        pathVertices[2] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[2].transform.position.x, screenCorners[2].transform.position.y, -9));
        pathVertices[2] = new Vector3(pathVertices[2].x + paddledWidth, pathVertices[2].y + paddledWidth, 0);

        //Bottom Side - Right
        pathVertices[3] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[3].transform.position.x, screenCorners[3].transform.position.y, -9));
        pathVertices[3] = new Vector3(pathVertices[3].x - paddledWidth, pathVertices[3].y + paddledWidth, 0);

        //Right Side - Bottom
        pathVertices[4] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[4].transform.position.x, screenCorners[4].transform.position.y, -9));
        pathVertices[4] = new Vector3(pathVertices[4].x - paddledWidth, pathVertices[4].y + paddledWidth, 0);

        //Right Side - Top
        pathVertices[5] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[5].transform.position.x, screenCorners[5].transform.position.y, -9));
        pathVertices[5] = new Vector3(pathVertices[5].x - paddledWidth, pathVertices[5].y - paddledWidth, 0);

        //Top Side - Right
        pathVertices[6] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[6].transform.position.x, screenCorners[6].transform.position.y, -9));
        pathVertices[6] = new Vector3(pathVertices[6].x - paddledWidth, pathVertices[6].y - paddledWidth, 0);

        //Top Side - Left
        pathVertices[7] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[7].transform.position.x, screenCorners[7].transform.position.y, -9));
        pathVertices[7] = new Vector3(pathVertices[7].x + paddledWidth, pathVertices[7].y - paddledWidth, 0);
    }

    void Start()
    {
        score = 0;
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        AddPaddles();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (paddlesReady > 1 && !isPlaying)
            {
                AddBall();
                isPlaying = true;
                uiManager.FadeInScore();
            }
        }
    }

    private void AddPaddles()
    {
        Paddle paddleLeft = Instantiate(paddle) as Paddle;
        paddles.Add(paddleLeft);

        Paddle paddleRight = Instantiate(paddle) as Paddle;
        paddles.Add(paddleRight);

        paddleLeft.Init(0, this, paddledWidth);
        paddleRight.Init(4, this, paddledWidth);
    }

    public void ChangePaddleDirection(int dir)
    {
        foreach(var paddle in paddles)
        {
            paddle.ChangeDirection(dir);
        }
    }

    public void AddPaddlePoint()
    {
        foreach (var paddle in paddles)
        {
            paddle.AddPaddlePoint();
        }
    }

    public void SetUnitDistance(float uDist)
    {
        pc.SetUnitDistance(uDist);
    }

    public void EarnPoint()
    {
        score++;

        if (score % 10 == 0)
        {
            bool didDraw = mlr.DrawShape();

            if (didDraw)
            {
                uiManager.FadeOutScore();
            }
            else
            {
                uiManager.FadeInScore();
            }
        }
        
        uiManager.ChangeScore(score);
    }

    public void CircleCollision(GameObject ball)
    {
        numHitCircle++;
        Color curColor = ball.gameObject.GetComponent<SpriteRenderer>().color;

        if (numHitCircle >= 3)
        {
            GameObject part = Instantiate(circleHitParticles);
            part.transform.position = this.gameObject.transform.position;
            var col = part.GetComponent<ParticleSystem>().colorOverLifetime;
            col.color = curColor;
            numHitCircle = 0;
        }
    }

    public void GameOver()
    {
        mlr.DestroyShape();
        uiManager.DisplayGameOver(score);

        if (!GameController.script.noAds)
        {
            StartCoroutine(displayAd());
        }
    }

    private IEnumerator displayAd()
    {
        yield return new WaitForSeconds(0.5f);
        waitingForAd = true;
        GameController.script.ShowAd(score);
    }

    public void PlayAgain()
    {
        if (!waitingForAd)
        {
            GameController.script.hasContinued = false;
            ReloadScene();
        }
    }

    public void Continue()
    {
        GameController.script.ShowRewardedAd();
    }

    public void BuyRemoveAds()
    {
        GameController.script.BuyRemoveAds();
    }

    public void SuccessfulContinue()
    {
        uiManager.ContinueGame();
        paddlesReady = 0;
        paddles[0].gameObject.GetComponent<Paddle>().DestroyLine();
        paddles[1].gameObject.GetComponent<Paddle>().DestroyLine();
    }

    public void ContinueGame()
    {
        if (continueReady != 1)
        {
            continueReady++;
        }
        else
        {
            StartCoroutine(StartContinuedGame(2));
            continueReady++;
        }
    }

    private IEnumerator StartContinuedGame(int secs)
    {
        paddles.Clear();
        yield return new WaitForSeconds(secs);
        AddPaddles();
    }

    private void ReloadScene()
    {
        Scene loadedLevel = SceneManager.GetActiveScene();
        SceneManager.LoadScene(loadedLevel.buildIndex);
    }

    public void DeleteBall(GameObject self)
    {
        Destroy(self);
        ballCount--;

        if(ballCount <= 0)
        {
            GameOver();
        }
    }

    public void AddBall()
    {
        Ball theBall = Instantiate(ball) as Ball;
        theBall.AssignGameManager(this);
        soundManager.PlayBallSpawn();
        ballCount++;
    }

    public void BeginGame()
    {
            paddlesReady++;
    }

    public Themes getTheme()
    {
        return themes[selectedTheme];
    }
}
