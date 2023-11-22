using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Newtonsoft.Json;

public class FacebookController : MonoBehaviour
{

    //public GameObject ProfileImageObject;
    //public GameObject ProfileTextObject;
    public GameObject ScoreTextObject;
    //private Image profileImage;
    //private Text profileText;
    private Text scoreText;

    public Text LoginText;
    public Color ClickColor;
    public Color DefaultColor;
    private GameObject uiGameObject;

    void Awake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            FB.ActivateApp();
        }
    }

    void Start()
    {
        Initialize();
        if (FB.IsLoggedIn)
        {
            DisplayLeaderboard();
        }
        else
        {
            DisplayLoginFeature();
        }
    }

    private void Initialize()
    {
        uiGameObject = GameObject.FindGameObjectWithTag("UI");
        scoreText = ScoreTextObject.GetComponent<Text>();
        //profileImage = ProfileImageObject.GetComponent<Image>();
        //profileText = ProfileTextObject.GetComponent<Text>();
        //scoreText = ScoreTextObject.GetComponent<Text>();
    }

    private void DisplayLeaderboard()
    {
        uiGameObject.transform.Find("Login").gameObject.SetActive(false);
        uiGameObject.transform.Find("Table").gameObject.SetActive(true);
        LoadTable();
    }

    private void DisplayLoginFeature()
    {
        uiGameObject.transform.Find("Table").gameObject.SetActive(false);
        uiGameObject.transform.Find("Login").gameObject.SetActive(true);
    }

    public void LoginClicked()
    {
        LoginText.color = ClickColor;
    }

    public void LoginReleased()
    {
        LoginText.color = DefaultColor;
        FB.LogInWithReadPermissions(new List<string> { "public_profile", "user_friends" }, AuthCallback);
        FB.LogInWithPublishPermissions(new List<string> { "publish_actions" }, AuthCallback);
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            DisplayLeaderboard();
        }
    }

    private void LoadTable()
    {
        RetrieveAndSetScores();
    }

    public void RetrieveAndSetScores()
    {
        if (FB.IsLoggedIn)
        {
            FB.API("/app/scores?fields=score&access_token=" + AccessToken.CurrentAccessToken.TokenString, HttpMethod.GET, GetScoreText);
        }
    }

    private void GetScoreText(IGraphResult graphResult)
    {
        scoreText.text = "";
        int oldScore = 0;
        var results = JsonConvert.DeserializeObject<Rootobject>(graphResult.RawResult);
        var orderedResults = results.data.OrderBy(datum => datum.score).Reverse();

        foreach (var result in orderedResults)
        {
            if (result.user.id != AccessToken.CurrentAccessToken.UserId)
            {
                scoreText.text += result.score.ToString(@"D7") + " " + result.user.name + "\r\n";
            }
            else
            {
                oldScore = result.score;
                var newScore = PlayerPrefs.GetInt("Score");
                if (newScore > oldScore)
                {
                    SetScore(newScore);
                    scoreText.text += "<color=#E1DC3DFF><b>" + newScore.ToString(@"D7") + " " + result.user.name + "</b></color> \r\n";
                }
                else
                {
                    scoreText.text += "<color=#E1DC3DFF><b>" + oldScore.ToString(@"D7") + " " + result.user.name + "</b></color> \r\n";
                }


            }
        }
        //UPDATE YOUR NEW SCORE IS: 321344
    }

    public class Rootobject
    {
        public Datum[] data { get; set; }
    }

    public class Datum
    {
        public int score { get; set; }
        public User user { get; set; }
    }

    public class User
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void SetScore(int score)
    {
        if (FB.IsLoggedIn)
        {
            FB.API("/me/scores?fields=score&access_token=" + AccessToken.CurrentAccessToken.TokenString, HttpMethod.POST, null, new Dictionary<string, string> { { "score", score.ToString() } });
        }
    }

    //private void SetProfilePicture(AccessToken accessToken)
    //{
    //    FB.API("/me/picture?type=square&height=256&width=256&access_token=" + accessToken.TokenString, HttpMethod.GET, DisplayProfilePic);
    //}

    //private void SetProfileText(AccessToken accessToken)
    //{
    //    FB.API("me?fields=name&access_token=" + accessToken.TokenString, HttpMethod.GET, DisplayName);
    //}

    //private void DisplayProfilePic(IGraphResult result)
    //{
    //    if (result.Texture != null)
    //    {
    //        profileImage.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 256, 256), new Vector2());
    //    }
    //    else
    //    {
    //        Debug.Log("no result");
    //    }
    //}

    //private void DisplayName(IGraphResult result)
    //{
    //    if (result.Texture != null)
    //    {
    //        //profileText.text = result.ResultDictionary["name"].ToString();
    //    }
    //    else
    //    {
    //        Debug.Log("no result");
    //    }
    //}

    //public void Logout()
    //{
    //    FB.API("/me/permissions?access_token=" + AccessToken.CurrentAccessToken.TokenString, HttpMethod.DELETE);
    //}
}