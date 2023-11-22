using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour
{
    public Text Score;
    public Statistics Statistics;
    private int playerScore;

    void Start()
    {
        UpdateScoreText();
    }

    public void SetPlayerScore(int addScore)
    {
        Statistics.Score += addScore;
        playerScore += addScore;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        Score.text = "Score: " + playerScore;
    }
}