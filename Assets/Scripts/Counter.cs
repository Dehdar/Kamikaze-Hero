using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows how many seconds the player has survived.
/// </summary>
public class Counter : MonoBehaviour
{
    private GameManager gameManager;
    private Text counterText;
    private float counter;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag(Tags.GameManager).GetComponent<GameManager>();
        counterText = gameObject.GetComponent<Text>();
    }

    void Update()
    {
        if (gameManager.IsPlayerAlive())
        {
            counter += Time.deltaTime;
            counterText.text = counter.ToString("0") + " sec";
        }
    }
}
