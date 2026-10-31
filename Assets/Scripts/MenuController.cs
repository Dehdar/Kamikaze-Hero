using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Button handlers for the Menu, About and Game-over screens (wired via EventTrigger in each scene).
/// The *Clicked methods highlight the label on press; the *Released methods act on release.
/// </summary>
public class MenuController : MonoBehaviour
{
    public Text StartText;
    public Text AboutText;
    public Text ExitText;
    public Text BackToMenuArrowText;
    public Text RestartText;
    public Image BackgroundLoader;
    public Color ClickColor;
    public Color DefaultColor;
    public BackgroundMover BackgroundMover;
    private Animator animator;
    private GameObject internetErrorDialogue;

    // Show an interstitial ad on every third game started (from the menu or via restart).
    // Static so the count survives scene loads.
    private const int GamesPerInterstitial = 3;
    private static int gamesStarted;

    void Start()
    {
        internetErrorDialogue = GameObject.FindGameObjectWithTag(Tags.InternetErrorDialogue);
        var menuGameObject = GameObject.FindGameObjectWithTag(Tags.Menu);
        if (menuGameObject != null)
        {
            animator = menuGameObject.GetComponent<Animator>();
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
        CountGameAndMaybeShowAd();
        StartText.color = DefaultColor;
        animator.SetTrigger("InitiateGame");
        SceneManager.LoadSceneAsync(SceneNames.Game, LoadSceneMode.Single);
    }

    public void AboutClicked()
    {
        AboutText.color = ClickColor;
    }

    public void AboutReleased()
    {
        AboutText.color = DefaultColor;
        SceneManager.LoadScene(SceneNames.About, LoadSceneMode.Single);
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
        SceneManager.LoadScene(SceneNames.Menu, LoadSceneMode.Single);
    }

    public void RestartTextClicked()
    {
        RestartText.color = ClickColor;
    }

    public void RestartTextReleased()
    {
        BackgroundMover.Reset();
        CountGameAndMaybeShowAd();
        RestartText.color = DefaultColor;
        SceneManager.LoadScene(SceneNames.Game, LoadSceneMode.Single);
    }

    private static void CountGameAndMaybeShowAd()
    {
        gamesStarted++;
        if (gamesStarted % GamesPerInterstitial == 0)
        {
            AdsManager.ShowInterstitial();
        }
    }

    public void CloseInternetErrorDialogue()
    {
        internetErrorDialogue.SetActive(false);
    }
}
