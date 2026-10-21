using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuController : MonoBehaviour
{
    public Text StartText;
    public Text AboutText;
    public Text ExitText;
    public Text BackToMenuArrowText;
    public Text RestartText;
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

        AdsManager.Initialize();
    }

    public void StartClicked()
    {
        StartText.color = ClickColor;
        BackgroundLoader.enabled = true;
    }

    public void StartReleased()
    {
        if (++GameRetries % 3 == 0)
        {
            AdsManager.ShowInterstitial();
        }
       
        StartText.color = DefaultColor;
        animator.SetTrigger("InitiateGame");
        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
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
        if (++GameRetries % 3 == 0)
        {
            AdsManager.ShowInterstitial();
        }
        RestartText.color = DefaultColor;
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void CloseInternetErrorDialogue()
    {
        internetErrorDialogue.SetActive(false);
    }
}