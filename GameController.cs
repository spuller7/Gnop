using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public class GameController : MonoBehaviour, IUnityAdsListener
{
    public static GameController script;

    private string playstore_id = "3549823";
    private string appstore_id = "3549822";
    public bool isPlayStore;
    public bool isTest;

    public enum GameMode {Classic, Chaos, Impossible, Unfair}

    GameMode gameMode = GameMode.Classic;

    public bool hasContinued = false;
    public int adCounter = 1;
    public int gamesNeededForAd = 3;
    private int highscore;

    public bool noAds = false;
    public bool unlockedChaos = false;
    public bool unlockedImpossible = false;
    public bool unlockedUnfair = false;

    public float speed = 0f;
    public int score = 0;
    public bool isContinue = false;

    private void Awake()
    {
        if (script == null)
        {
            DontDestroyOnLoad(gameObject);
            script = this;
        }
        else if (script != this)
        {
            Destroy(gameObject);
        }

        highscore = PlayerPrefs.GetInt("HighScore", 0);
        UpdateHighScore(highscore);
        //Screen.SetResolution(1024, 1280, false);
    }

    private void Start()
    {
        //Initialize IAP
        IAPManager.Instance.InitializeIAPManager(InitializeResultCallback);
        Restore();

        InitialiazeMonetization();
    }

    public void Restore()
    {
        IAPManager.Instance.RestorePurchases(ProductRestoredCallback);
    }

    private void InitialiazeMonetization()
    {
        Advertisement.AddListener(this);
        if (isPlayStore)
        {
            Advertisement.Initialize(playstore_id, isTest);
            return;
        }
        Advertisement.Initialize(appstore_id, isTest);
    }

    public void PlayGame(GameMode mode)
    {
        gameMode = mode;
        highscore = GetHighScore();
        SceneManager.LoadScene("Main");
    }

    public void ShowAd()
    {
        if (!noAds)
        {
            if (adCounter >= gamesNeededForAd)
            {
                int rand_show = Random.Range(1,4);

                if (rand_show != 1)
                {
                    Advertisement.Show();

                    adCounter = 0;
                }
            }
            else
            {
                adCounter++;
            }

            //GameObject.Find("Game Manager").GetComponent<GameManager>().waitingForAd = false;
        }
    }

    public void ShowRewardedAd()
    {
        Advertisement.Show("rewardedVideo");
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished && placementId == "rewardedVideo")
        {
            hasContinued = true;
            GameObject.Find("Game Manager").GetComponent<GameManager>().SuccessfulContinue();
        }
    }

    public void OnUnityAdsReady(string placementId)
    {
        // If the ready Placement is rewarded, activate the button: 
        if (placementId == "rewardedVideo")
        {
            //myButton.interactable = true;
        }
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
        Debug.Log(message);
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        // Optional actions to take when the end-users triggers an ad.
    }

    public bool CanContinue(int score)
    {
        if (hasContinued)
        {
            hasContinued = false;
            return false;
        }

        if (score > (highscore * 0.8) || score < (highscore * 0.35))
        {
            return false;
        }

        return true;
    }

    public void UpdateHighScore(int score)
    {
        if (gameMode == GameMode.Chaos)
        {
            PlayerPrefs.SetInt("ChaosHighScore", score);
        }
        else if (gameMode == GameMode.Impossible)
        {
            PlayerPrefs.SetInt("ImpossibleHighScore", score);
        }
        else if (gameMode == GameMode.Unfair)
        {
            PlayerPrefs.SetInt("UnfairHighScore", score);
        }
        else
        {
            PlayerPrefs.SetInt("HighScore", score);
        }
        
        highscore = score;
        CloudOnceServices.instance.SubmitScore(highscore);
    }

    public int GetHighScore()
    {
        if (gameMode == GameMode.Classic)
        {
            return PlayerPrefs.GetInt("HighScore", 0);
        }
        else if (gameMode == GameMode.Chaos)
        {
            return PlayerPrefs.GetInt("ChaosHighScore", 0);
        }
        else if (gameMode == GameMode.Impossible)
        {
            return PlayerPrefs.GetInt("ImpossibleHighScore", 0);
        }
        else if (gameMode == GameMode.Unfair)
        {
            return PlayerPrefs.GetInt("UnfairHighScore", 0);
        }

        return 0;
    }

    private void InitializeResultCallback(IAPOperationStatus status, string message, List<StoreProduct> shopProducts)
    {
        if (status == IAPOperationStatus.Success)
        {
            //IAP was successfully initialized
            //loop through all products
            for (int i = 0; i < shopProducts.Count; i++)
            {
                //if active variable is true, means that user had bought that product
                //so enable access
                if (shopProducts[i].active)
                {
                    if (shopProducts[i].productName == "RemoveAds")
                    {
                        noAds = true;
                    }
                    if (shopProducts[i].productName == "UnlockChaos")
                    {
                        unlockedChaos = true;
                    }
                    if (shopProducts[i].productName == "UnlockImpossible")
                    {
                        unlockedImpossible = true;
                    }
                    if (shopProducts[i].productName == "UnlockUnfair")
                    {
                        unlockedUnfair = true;
                    }
                }
            }

            GameObject.Find("Game Mode Manager").GetComponent<GameModeManager>().UpdateGameModeDisplay();
        }
    }

    private void ProductRestoredCallback(IAPOperationStatus status, string message, StoreProduct product)
    {
        if (status == IAPOperationStatus.Success)
        {
            if (product.productName == "RemoveAds")
                noAds = true;
            else if (product.productName == "UnlockChaos")
                unlockedChaos = true;
            else if (product.productName == "UnlockImpossible")
                unlockedImpossible = true;
            else if (product.productName == "UnlockUnfair")
                unlockedUnfair = true;

            GameObject.Find("Game Mode Manager").GetComponent<GameModeManager>().UpdateGameModeDisplay();
        }
    }

    public void BuyRemoveAds()
    {
        IAPManager.Instance.BuyProduct(ShopProductNames.RemoveAds, ProductBoughtCallback);
    }

    public void BuyGameMode(GameMode gameM)
    {
        if (gameM == GameMode.Chaos)
            IAPManager.Instance.BuyProduct(ShopProductNames.UnlockChaos, ProductBoughtCallback);
        else if (gameM == GameMode.Impossible)
            IAPManager.Instance.BuyProduct(ShopProductNames.UnlockImpossible, ProductBoughtCallback);
        else if (gameM == GameMode.Unfair)
            IAPManager.Instance.BuyProduct(ShopProductNames.UnlockUnfair, ProductBoughtCallback);
    }

    private void ProductBoughtCallback(IAPOperationStatus status, string message, StoreProduct product)
    {
        if (status == IAPOperationStatus.Success)
        {
            if (product.productName == "RemoveAds")
            {
                noAds = true;
                return;
            }
            else if (product.productName == "UnlockChaos")
                unlockedChaos = true;
            else if (product.productName == "UnlockImpossible")
                unlockedImpossible = true;
            else if (product.productName == "UnlockUnfair")
                unlockedUnfair = true;
            
            GameObject.Find("Game Mode Manager").GetComponent<GameModeManager>().UpdateGameModeDisplay();
        }
    }

    public GameMode GetGameMode()
    {
        return gameMode;
    }

    public void TestPurchase()
    {
        IAPManager.Instance.BuyProduct(ShopProductNames.Test, ProductBoughtCallback);
    }
}
