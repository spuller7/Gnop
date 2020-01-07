using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Monetization;

public class GameController : MonoBehaviour
{
    public static GameController script;

    private string playstore_id = "3068591";
    private string appstore_id = "3068590";
    public bool isPlayStore;
    public bool isTest;

    public bool hasContinued = false;
    public int adCounter = 1;
    public int gamesNeededForAd = 2;
    public int highscore;

    public bool noAds = false;

    private void Awake()
    {
        if (script == null)
        {
            DontDestroyOnLoad(gameObject);
            script = this;
        }
        else if(script != this)
        {
            Destroy(gameObject);
        }

        highscore = PlayerPrefs.GetInt("HighScore", 0);
        //Screen.SetResolution(1200, 800, false);
    }

    private void Start()
    {
        //Initialize IAP
        IAPManager.Instance.InitializeIAPManager(InitializeResultCallback);
        IAPManager.Instance.RestorePurchases(ProductRestoredCallback);

        InitialiazeMonetization();
    }

    private void InitialiazeMonetization()
    {
        if(isPlayStore)
        {
            Monetization.Initialize(playstore_id, isTest);
            return;
        }
        Monetization.Initialize(appstore_id , isTest);
    }

    public void playGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void ShowAd(int score)
    {
        if (!noAds)
        {
            if (adCounter >= gamesNeededForAd)
            {
                if (Monetization.IsReady("video"))
                {
                    ShowAdPlacementContent videoAd = null;
                    videoAd = Monetization.GetPlacementContent("video") as ShowAdPlacementContent;

                    if (videoAd != null)
                    {
                        videoAd.Show();
                    }
                }

                adCounter = 1;
                if (gamesNeededForAd == 2)
                {
                    gamesNeededForAd = 3;
                }
                else { gamesNeededForAd = 2; }
            }
            else
            {
                adCounter++;
            }

            GameObject.Find("Game Manager").GetComponent<GameManager>().waitingForAd = false;
        }
    }

    public void ShowRewardedAd()
    {
        if (Monetization.IsReady("rewardedVideo"))
        {
            ShowAdPlacementContent rewardedVideoAd = null;
            rewardedVideoAd = Monetization.GetPlacementContent("rewardedVideo") as ShowAdPlacementContent;

            if (rewardedVideoAd != null)
            {
                rewardedVideoAd.Show(HandleShowResult);
            }
        }
    }

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                hasContinued = true;
                GameObject.Find("Game Manager").GetComponent<GameManager>().SuccessfulContinue();
                break;
            default:
                GameObject.Find("Game Manager").GetComponent<GameManager>().PlayAgain();
                break;
        }
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
        PlayerPrefs.SetInt("HighScore", score);
        highscore = score;
    }

    private void InitializeResultCallback(IAPOperationStatus status, string message, List<StoreProduct> shopProducts)
    {
        if (status == IAPOperationStatus.Success)
        {
            //IAP was successfully initialized
            //loop through all products
            for (int i = 0; i < shopProducts.Count; i++)
            {
                if (shopProducts[i].productName == "RemoveAds")
                {
                    //if active variable is true, means that user had bought that product
                    //so enable access
                    if (shopProducts[i].active)
                    {
                        noAds = true;
                    }
                }
            }
        }
    }

    private void ProductRestoredCallback(IAPOperationStatus status, string message, StoreProduct product)
    {
        if (status == IAPOperationStatus.Success)
        {
            if (product.productName == "RemoveAds")
            {
                noAds = true;
            }
        }
    }

    public void BuyRemoveAds()
    {
        IAPManager.Instance.BuyProduct(ShopProductNames.RemoveAds, ProductBoughtCallback);
    }

    private void ProductBoughtCallback(IAPOperationStatus status, string message, StoreProduct product)
    {
        if (status == IAPOperationStatus.Success)
        {
            if (product.productName == "RemoveAds")
                noAds = true;
        }
    }
}
