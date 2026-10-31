using UnityEngine;
using UnityEngine.UI;

public class ButtonLoader : MonoBehaviour
{
    public float LoadTime;
    private Image imageLoader;
    private bool loadFirstHalfFinished;
    private bool startLoad;

    void Start()
    {
        imageLoader = GetComponent<Image>();
    }

    void Update()
    {
        if (startLoad)
        {
            HandleUnload();
            HandleReload();
        }
    }

    public void LoadButton()
    {
        startLoad = true;
    }

    private void HandleUnload()
    {
        if (!loadFirstHalfFinished)
        {
            imageLoader.fillAmount -= Time.deltaTime/ LoadTime;
            if (imageLoader.fillAmount <= 0)
            {
                imageLoader.fillClockwise = !imageLoader.fillClockwise;
                loadFirstHalfFinished = true;
            }
        }
    }

    private void HandleReload()
    {
        if (loadFirstHalfFinished)
        {
            imageLoader.fillAmount += Time.deltaTime/ LoadTime;
            if (imageLoader.fillAmount >= 1)
            {
                imageLoader.fillClockwise = !imageLoader.fillClockwise;
                loadFirstHalfFinished = false;
                startLoad = false;
            }
        }
    }
}
