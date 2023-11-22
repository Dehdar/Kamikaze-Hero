using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;


public class MenuController : MonoBehaviour
{
    public Text StartText;
    public Text LeaderboardText;
    public Text AboutText;
    public Text ExitText;
    public Text BackToMenuArrowText;
    public Text RestartText;
    public Text SaveText;
    public Statistics Statistics;
    public Image BackgroundLoader;
    public Color ClickColor;
    public Color DefaultColor;
    public BackgroundMover BackgroundMover;
    private Animator animator;
    private GameObject internetErrorDialogue;
    public static int GameRetries = 0;

    void Start()
    {
        internetErrorDialogue = GameObject.FindGameObjectWithTag("InternetErrorDialogue");
        var menuGameObject = GameObject.FindGameObjectWithTag("Menu");
        if (menuGameObject != null)
        {
            animator = GameObject.FindGameObjectWithTag("Menu").GetComponent<Animator>();
            internetErrorDialogue = menuGameObject.transform.Find("InternetError").gameObject;
        }
    }

    public void StartClicked()
    {
        StartText.color = ClickColor;
        BackgroundLoader.enabled = true;
    }

    public void StartReleased()
    {
        if ((++GameRetries % 3 == 0) && Advertisement.IsReady())
        {
            Advertisement.Show();
        }
       
        StartText.color = DefaultColor;
        animator.SetTrigger("InitiateGame");
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
    }

    public void LeaderboardClicked()
    {
        LeaderboardText.color = ClickColor;
    }

    public void LeaderboardReleased()
    {
        LeaderboardText.color = DefaultColor;
        StartCoroutine(CheckInternetConnection(isConnected =>
        {
            if (isConnected)
            {
                SceneManager.LoadScene("Leaderboard", LoadSceneMode.Single);
            }
            else
            {
                OpenInternetErrorDialogue();
            }
        }));
    }

    public void AboutClicked()
    {
        AboutText.color = ClickColor;
    }

    public void AboutReleased()
    {
        AboutText.color = DefaultColor;
        SceneManager.LoadScene("About", LoadSceneMode.Single);
    }

    public void ExitClicked()
    {
        ExitText.color = ClickColor;
    }

    public void ExitReleased()
    {
        ExitText.color = DefaultColor;
        Application.Quit();
    }

    public void BackToMenuArrowTextClicked()
    {
        BackToMenuArrowText.color = ClickColor;
    }

    public void BackToMenuArrowTextReleased()
    {
        BackToMenuArrowText.color = DefaultColor;
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void RestartTextClicked()
    {
        RestartText.color = ClickColor;
    }

    public void RestartTextReleased()
    {
        BackgroundMover.Reset();
        if ((++GameRetries % 3 == 0) && Advertisement.IsReady())
        {
            Advertisement.Show();
        }
        RestartText.color = DefaultColor;
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void SaveTextClicked()
    {
        SaveText.color = ClickColor;
    }

    public void SaveTextReleased()
    {
        SaveText.color = DefaultColor;
        StartCoroutine(CheckInternetConnection(isConnected =>
        {
            if (isConnected)
            {
                PlayerPrefs.SetInt("Score", Statistics.Score);
                SceneManager.LoadScene("Leaderboard", LoadSceneMode.Single);
            }
            else
            {
                OpenInternetErrorDialogue();
            }
        }));
    }

    public void CloseInternetErrorDialogue()
    {
        internetErrorDialogue.SetActive(false);
    }

    private void OpenInternetErrorDialogue()
    {
        internetErrorDialogue.SetActive(true);
    }

    IEnumerator CheckInternetConnection(Action<bool> action)
    {
        WWW www = new WWW("https://www.google.com/");
        yield return www;
        action(www.error == null);
    }
}