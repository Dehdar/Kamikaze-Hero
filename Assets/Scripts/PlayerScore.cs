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

    public void AddScore(int points)
    {
        Statistics.Score += points;
        playerScore += points;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        Score.text = "Score: " + playerScore;
    }
}
