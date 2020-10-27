using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using E7.NotchSolution;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    public UIManager uiManager;

    [SerializeField]
    public SoundManager soundManager;

    [SerializeField]
    private MiddleLineRenderer mlr;

    [Header("Color Themes")]
    public Themes[] themes;

    [SerializeField]
    private float minSpeed;

    [SerializeField]
    private float maxSpeed;

    [SerializeField]
    private float speedAcceleration;

    private float speed = 1;

    [SerializeField]
    private float paddleSpeedMultiplier = 1.5f;

    private float elapsedDistance = 0;
    private float unitDistance;

    [SerializeField]
    private int pointSeparation;

    private int selectedTheme;

    [Header("Misc")]

    [SerializeField]
    private GameObject circleHitParticles;

    [SerializeField]
    private float paddledWidth = 0.2f;

    [SerializeField]
    private GameObject[] screenCorners = new GameObject[8];

    [SerializeField]
    Paddles paddles;

    [SerializeField]
    private GameObject powerup;

    [SerializeField]
    private int powerupLife = 12;

    [SerializeField]
    private GameObject chaosDescription;

    private List<Powerup> powerups = new List<Powerup>();

    public int score = 0;
    public Ball ball;
    private int ballCount;
    private int continueReady = 0;
    public bool readyToStart = false;
    private int numHitCircle = 0;
    private bool isObstacle = false;

    public static Vector2 bottomLeft;
    public static Vector2 topRight;

    public Vector3[] pathVertices = new Vector3[8];
    public Vector3[] safetyVertices = new Vector3[8];

    private bool isPlaying = false;
    public bool isDead = false;
    public bool waitingForAd = false;
    public GameObject moveObject;
    private Vector3 startPosition;
    private int elapsedUnits = 0;
    private int cornerCurveDistance = 70;

    private int fakeBallCount = 0;
    private bool waitingTapToStart = false;

    private void Awake()
    {
        //selectedTheme = Random.Range(0, themes.Length);
        selectedTheme = 0;

        Camera.main.backgroundColor = themes[selectedTheme].backgroundColor;
        Rect safeRect = Screen.safeArea;
        float paddingTop = Screen.height - safeRect.yMax;
        float paddingBottom = safeRect.y;

        //Left Side - Top
        //screenCorners[0].transform.position = new Vector3(screenCorners[0].transform.position.x, screenCorners[0].transform.position.y - paddingTop);
        pathVertices[0] = Camera.main.ScreenToWorldPoint(screenCorners[0].transform.position);
        pathVertices[0] = new Vector3(pathVertices[0].x + paddledWidth, pathVertices[0].y - paddledWidth, 0);

        safetyVertices[0] = Camera.main.ScreenToWorldPoint(screenCorners[0].transform.position);
        safetyVertices[0] = new Vector3(pathVertices[0].x - (paddledWidth / 2), pathVertices[0].y, 0);

        //Left Side - Bottom
        //screenCorners[1].transform.position = new Vector3(screenCorners[1].transform.position.x, screenCorners[1].transform.position.y + paddingBottom);
        pathVertices[1] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[1].transform.position.x, screenCorners[1].transform.position.y, -9));
        pathVertices[1] = new Vector3(pathVertices[1].x + paddledWidth, pathVertices[1].y + paddledWidth, 0);

        safetyVertices[1] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[1].transform.position.x, screenCorners[1].transform.position.y, -9));
        safetyVertices[1] = new Vector3(pathVertices[1].x - (paddledWidth / 2), pathVertices[1].y, 0);

        //Bottom Side - Left
        //screenCorners[2].transform.position = new Vector3(screenCorners[2].transform.position.x, screenCorners[2].transform.position.y + paddingBottom);
        pathVertices[2] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[2].transform.position.x, screenCorners[2].transform.position.y, -9));
        pathVertices[2] = new Vector3(pathVertices[2].x + paddledWidth, pathVertices[2].y + paddledWidth, 0);

        safetyVertices[2] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[2].transform.position.x, screenCorners[2].transform.position.y, -9));
        safetyVertices[2] = new Vector3(pathVertices[2].x, pathVertices[2].y - (paddledWidth / 2), 0);

        //Bottom Side - Right
        //screenCorners[3].transform.position = new Vector3(screenCorners[3].transform.position.x, screenCorners[3].transform.position.y + paddingBottom);
        pathVertices[3] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[3].transform.position.x, screenCorners[3].transform.position.y, -9));
        pathVertices[3] = new Vector3(pathVertices[3].x - paddledWidth, pathVertices[3].y + paddledWidth, 0);

        safetyVertices[3] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[3].transform.position.x, screenCorners[3].transform.position.y, -9));
        safetyVertices[3] = new Vector3(pathVertices[3].x, pathVertices[3].y - (paddledWidth / 2), 0);

        //Right Side - Bottom
        //screenCorners[4].transform.position = new Vector3(screenCorners[4].transform.position.x, screenCorners[4].transform.position.y + paddingBottom);
        pathVertices[4] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[4].transform.position.x, screenCorners[4].transform.position.y, -9));
        pathVertices[4] = new Vector3(pathVertices[4].x - paddledWidth, pathVertices[4].y + paddledWidth, 0);

        safetyVertices[4] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[4].transform.position.x, screenCorners[4].transform.position.y, -9));
        safetyVertices[4] = new Vector3(pathVertices[4].x + (paddledWidth / 2), pathVertices[4].y, 0);

        //Right Side - Top
        //screenCorners[5].transform.position = new Vector3(screenCorners[5].transform.position.x, screenCorners[5].transform.position.y - paddingTop);
        pathVertices[5] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[5].transform.position.x, screenCorners[5].transform.position.y, -9));
        pathVertices[5] = new Vector3(pathVertices[5].x - paddledWidth, pathVertices[5].y - paddledWidth, 0);

        safetyVertices[5] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[5].transform.position.x, screenCorners[5].transform.position.y, -9));
        safetyVertices[5] = new Vector3(pathVertices[5].x + (paddledWidth / 2), pathVertices[5].y, 0);

        //Top Side - Right
        //screenCorners[6].transform.position = new Vector3(screenCorners[6].transform.position.x, screenCorners[6].transform.position.y - paddingTop);
        pathVertices[6] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[6].transform.position.x, screenCorners[6].transform.position.y, -9));
        pathVertices[6] = new Vector3(pathVertices[6].x - paddledWidth, pathVertices[6].y - paddledWidth, 0);

        safetyVertices[6] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[6].transform.position.x, screenCorners[6].transform.position.y, -9));
        safetyVertices[6] = new Vector3(pathVertices[6].x, pathVertices[6].y + (paddledWidth / 2), 0);

        //Top Side - Left
        //screenCorners[7].transform.position = new Vector3(screenCorners[7].transform.position.x, screenCorners[7].transform.position.y - paddingTop);
        pathVertices[7] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[7].transform.position.x, screenCorners[7].transform.position.y, -9));
        pathVertices[7] = new Vector3(pathVertices[7].x + paddledWidth, pathVertices[7].y - paddledWidth, 0);

        safetyVertices[7] = Camera.main.ScreenToWorldPoint(new Vector3(screenCorners[7].transform.position.x, screenCorners[7].transform.position.y, -9));
        safetyVertices[7] = new Vector3(pathVertices[7].x, pathVertices[7].y + (paddledWidth / 2), 0);

        float distance = Vector3.Distance(pathVertices[0], pathVertices[1]);
        unitDistance = distance / pointSeparation;
    }

    void Start()
    {
        if (GameController.script.GetGameMode() == GameController.GameMode.Chaos)
            chaosDescription.SetActive(true);
        else
            chaosDescription.SetActive(false);

        score = GameController.script.isContinue ? GameController.script.score : 0;
        uiManager.ChangeScore(score);
        speed = GameController.script.isContinue ? GameController.script.speed : minSpeed;

        if (GameController.script.isContinue)
        {
            waitingTapToStart = true;
            uiManager.ContinueGame();
            GameController.script.hasContinued = true;
            GameController.script.isContinue = false;
        }

        isPlaying = false;
        isDead = false;

        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        paddles.Init(this, paddledWidth, unitDistance);
    }

    public void BeginGame()
    {
        if (!waitingTapToStart)
        {
            startPosition = moveObject.transform.position;
            isPlaying = true;
            uiManager.FadeInScore();
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Switch Direction"))
        {
            if (waitingTapToStart)
            {
                waitingTapToStart = false;
                BeginGame();
            }
            else if (isPlaying && ballCount >= 1)
            {
                paddles.ChangeDirection();
            } 
        }

        if ((isPlaying || isDead) && !waitingTapToStart)
        {
            elapsedDistance = Vector3.Distance(moveObject.transform.position, startPosition);

            if ((elapsedDistance - (unitDistance * elapsedUnits)) >= unitDistance)
            {
                for (int i = 1; i <= (elapsedDistance - (unitDistance * elapsedUnits)) / unitDistance; i++)
                {
                    elapsedUnits += 1;
                    paddles.MovePaddles();
                }
            }

            if (!isDead && speed < maxSpeed)
            {
                speed += (speedAcceleration * Time.deltaTime);
            }
        }
    }

    private IEnumerator SpawnFakeBalls()
    {
        while (isPlaying)
        {
            yield return new WaitForSeconds(4 / speed);
            AddFakeBall(new Vector3(0, 0));
        }
    }

    public void EarnPoint()
    {
        if (!isPlaying)
            return;

        score++;

        // Add Obstacles
        if (GameController.script.GetGameMode() == GameController.GameMode.Classic)
        {
            if ((score + 1) % 15 == 0 && !isObstacle)
            {
                uiManager.FadeOutScore();
                mlr.DrawShape();
            }

            if (score % 15 == 0)
            {
                if (!isObstacle)
                {
                    isObstacle = true;
                    mlr.EnableLine();
                }
                else
                {
                    mlr.DrawShape();
                    uiManager.FadeInScore();
                    isObstacle = false;
                }
            }
        }
        else if (score == 1 && GameController.script.GetGameMode() == GameController.GameMode.Chaos)
        {
            StartCoroutine(SpawnFakeBalls());
            chaosDescription.SetActive(false);
        }
        else if (GameController.script.GetGameMode() == GameController.GameMode.Unfair)
        {
            if ((score + 1) % 25 == 0 && !isObstacle)
            {
                uiManager.FadeOutScore();
                mlr.DrawShape();
            }

            if (score % 25 == 0 && !isObstacle)
            {
                isObstacle = true;
                mlr.EnableLine();
            }

            if ((score - 10) % 25 == 0 && isObstacle)
            {
                mlr.DrawShape();
                uiManager.FadeInScore();
                isObstacle = false;
            }
        }
        else if (GameController.script.GetGameMode() == GameController.GameMode.Impossible)
        {
            if ((score + 1) % 10 == 0)
            {
                uiManager.FadeOutScore();
                mlr.DrawShape();
            }

            if (score % 10 == 0)
            {
                mlr.EnableLine();
                isObstacle = true;
            }
        }

        int spawnPowerup = Random.Range(0, 1000);

        if (spawnPowerup == 5)
        {
            float xVal = Random.Range(topRight.x, bottomLeft.x);
            float yVal = Random.Range(topRight.y, bottomLeft.y);

            GameObject newPowerup = Instantiate(powerup);

            newPowerup.transform.position = new Vector3(xVal / 1.5f, yVal / 1.5f, 0);

            Powerup p = newPowerup.GetComponent<Powerup>();
            p.init(score);
            powerups.Add(p);
        }

        foreach (Powerup p in powerups)
        {
            if (score - p.GetSpawnedScore() >= powerupLife)
            {
                powerups.Remove(p);
                Destroy(p.gameObject);
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
        chaosDescription.SetActive(false);
        isDead = true;
        isPlaying = false;
        mlr.DestroyShape(true);

        foreach (Powerup p in powerups)
        {
            Destroy(p.gameObject);
        }

        powerups.Clear();

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
        GameController.script.ShowAd();
    }

    public void PlayAgain()
    {
        GameController.script.hasContinued = false;
        ReloadScene();
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
        GameController.script.isContinue = true;
        GameController.script.score = score;
        GameController.script.speed = speed;
        ReloadScene();
    }

    public void ContinueGame()
    {
        if (continueReady != 1)
        {
            continueReady++;
        }
        else
        {
            continueReady++;
        }
    }

    private void ReloadScene()
    {
        Scene loadedLevel = SceneManager.GetActiveScene();
        SceneManager.LoadScene(loadedLevel.buildIndex);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void DeleteBall(GameObject self, bool isFake)
    {
        Destroy(self);

        if (isFake)
        {
            fakeBallCount--;
            return;
        }
        else
            ballCount--;

        if(ballCount <= 0)
            GameOver();
    }

    public void AddBall(Vector3 location)
    {
        Ball theBall = Instantiate(ball) as Ball;
        theBall.transform.position = location;
        theBall.Init(this);
        soundManager.PlayBallSpawn();
        ballCount++;
    }

    public void AddFakeBall(Vector3 location)
    {
        Ball theBall = Instantiate(ball) as Ball;
        theBall.transform.position = location;
        theBall.Init(this, true);
        fakeBallCount++;
    }

    public void HitPowerup(Powerup p)
    {
        powerups.Remove(p);
        AddBall(p.gameObject.transform.position);
    }
    
    //--------------------
    //READ ONLY FUNCTIONS
    //--------------------

    public float GetSpeed()
    {
        return speed;
    }

    public float GetPadddleSpeedMultiplier()
    {
        return paddleSpeedMultiplier;
    }

    public float GetScore()
    {
        return score;
    }

    public int GetDirection()
    {
        return paddles.GetDirection();
    }


    // Deprecated
    public Themes getTheme()
    {
        return themes[selectedTheme];
    }
}
