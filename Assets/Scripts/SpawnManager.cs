using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public float StartDelay;
    public float WaveDelay;
    public float SpawnDelayFactor;
    public int MaxWaves;
    public List<Object> Enemies;
    public Text WaveNumberText;
    public Statistics Statistics;
    private GameManager gameManager;
    private int waveNumber;
    private enum HorizontalSpace { Up, Down, Full };
    private Animator waveAnimator;
    static public bool PlayMusic = false;

    void Start()
    {
        waveNumber = 1;
        waveAnimator = (Animator)GameObject.FindGameObjectWithTag("Menu").GetComponent("Animator");
        gameManager = (GameManager)GameObject.FindGameObjectWithTag("GameManager").GetComponent("GameManager");
        WaveNumberText.text = "WAVE " + waveNumber + "/" + MaxWaves;
        waveAnimator.SetTrigger("DisplayWave");
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        PlayMusic = false;
        yield return new WaitForSeconds(StartDelay);

        while (gameManager.IsPlayerAlive() && waveNumber <= MaxWaves)
        {
            for (var i = 0; i < GetNumberOfEnemiesInWaves(); i++)
            {
                if (waveNumber >= 12)
                {
                    SpawnRandomEnemy(HorizontalSpace.Up);
                    SpawnRandomEnemy(HorizontalSpace.Down);
                }
                else
                {
                    SpawnRandomEnemy(HorizontalSpace.Full);
                }
                yield return new WaitForSeconds(GetSpawnDelay());
                if (!gameManager.IsPlayerAlive()) { break; }
            }
            if (!gameManager.IsPlayerAlive()) { break; }
            WaveNumberText.text = "WAVE " + ++waveNumber + "/" + MaxWaves;
            waveAnimator.SetTrigger("DisplayWave");
            yield return new WaitForSeconds(WaveDelay);
        }
        PlayMusic = true;
        if (gameManager.IsPlayerAlive())
        {
            GameCompleted();
        }
        else
        {
            GameOver();
        }
    }

    private void GameCompleted()
    {
        SetScore(true);
        AnimateEndGame();
    }

    private void GameOver()
    {
        Statistics.EnemiesMissed++;
        SetScore(false);
        AnimateEndGame();
    }

    private void SetScore(bool survived)
    {
        var menu = GameObject.FindGameObjectWithTag("Menu");
        var gameOver = menu.transform.Find("GameOver").gameObject;
        gameOver.transform.Find("GameOverText").GetComponent<Text>().text = survived ? "Game Complete" : "Game Over";
        gameOver.transform.Find("GameOverScore").GetComponent<Text>().text = ResolveScore();
    }

    private string ResolveScore()
    {
        var enemiesKilled = Statistics.EnemiesSpawned - Statistics.EnemiesMissed - 1;
        enemiesKilled = Mathf.Floor(enemiesKilled < 1 ? 0 : enemiesKilled);
        var burstsFired = Mathf.Ceil(Statistics.BurstsFired / 3f);
        return String.Format("\nScore: {0}\nEnemies killed: {3}\nEnemies missed: {4}\nBursts fired: {2}\nAccuracy: {1}%\nDied at wave: {5}\n\nVerdict: {6}",
            Statistics.Score,
            (enemiesKilled / (burstsFired < 1 ? 1 : burstsFired) * 100).ToString("F2"),
            burstsFired,
            enemiesKilled,
            Mathf.Floor(Statistics.EnemiesMissed),
            waveNumber,
            Evaluate(waveNumber));
    }

    private string Evaluate(int rank)
    {
        if (rank == 20)
        {
            return "GOD";
        }
        if (rank >= 18)
        {
            return "Unbelievable";
        }
        if (rank >= 15)
        {
            return "Insanely skilled";
        }
        if (rank >= 13)
        {
            return "Elite";
        }
        if (rank >= 11)
        {
            return "Kamikaze";
        }
        if (rank >= 9)
        {
            return "Journeyman";
        }
        if (rank >= 6)
        {
            return "Average Joe";
        }
        if (rank >= 3)
        {
            return "Babysitter";
        }

        return "n00b";
    }

    private static void AnimateEndGame()
    {
        var menu = GameObject.FindGameObjectWithTag("Menu");
        var gameOver = menu.transform.Find("GameOver").gameObject;
        if (gameOver != null)
        {
            gameOver.SetActive(true);
            gameOver.transform.Find("RestartButton").gameObject.SetActive(true);
            var animator = gameOver.GetComponent<Animator>();
            animator.SetTrigger("EndGame");
        }
    }

    private void SpawnRandomEnemy(HorizontalSpace horizontalSpace)
    {
        var randomEnemy = Enemies[GetShuffleNumber()];
        var position = new Vector3(16.5f, GetRandomEnemyHorizontalRange(randomEnemy, horizontalSpace), -0.9f);
        var rotation = Quaternion.AngleAxis(0, Vector3.down);
        Statistics.EnemiesSpawned++;
        Instantiate(randomEnemy, position, rotation);
    }

    private int GetNumberOfEnemiesInWaves()
    {
        if (waveNumber == 20)
            return 200;
        if (waveNumber < 10)
            return 5;

        return waveNumber + 5;
    }

    private int GetShuffleNumber()
    {
        if (waveNumber >= 6)
            return Random.Range(0, 3);
        if (waveNumber >= 3)
            return Random.Range(0, 2);

        return Random.Range(0, 1);
    }

    private float GetSpawnDelay()
    {
        if (waveNumber > 15)
        {
            return 1f;
        }
        return SpawnDelayFactor - waveNumber / 10;
    }

    private static float GetRandomEnemyHorizontalRange(Object enemy, HorizontalSpace horizontalSpace)
    {
        if (horizontalSpace == HorizontalSpace.Full)
        {
            if (((GameObject)enemy).tag == "Missile")
                return Random.Range(-4.19f, 5.15f);
            if (((GameObject)enemy).tag == "GuidedMissile")
                return Random.Range(-4.83f, 4.56f);
            if (((GameObject)enemy).tag == "MultiMissile")
                return Random.Range(-4.60f, 3.25f);
        }

        if (horizontalSpace == HorizontalSpace.Up)
        {
            if (((GameObject)enemy).tag == "Missile")
                return Random.Range(0.48f, 5.15f);
            if (((GameObject)enemy).tag == "GuidedMissile")
                return Random.Range(-0.135f, 4.56f);
            if (((GameObject)enemy).tag == "MultiMissile")
                return Random.Range(-0.675f, 3.25f);
        }

        if (horizontalSpace == HorizontalSpace.Down)
        {
            if (((GameObject)enemy).tag == "Missile")
                return Random.Range(-4.19f, 0.48f);
            if (((GameObject)enemy).tag == "GuidedMissile")
                return Random.Range(-4.83f, -0.135f);
            if (((GameObject)enemy).tag == "MultiMissile")
                return Random.Range(-4.60f, -0.675f);
        }

        return Random.Range(-4f, 3f);
    }
}
