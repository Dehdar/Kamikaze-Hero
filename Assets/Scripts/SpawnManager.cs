using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// Runs the game loop in the Game scene: spawns enemy waves, shows the wave banner,
/// and shows the end screen with the player's statistics when the run ends.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public float StartDelay;
    public float WaveDelay;
    public float SpawnDelayFactor;
    public int MaxWaves;
    // Enemy prefabs in unlock order: later entries only appear in later waves (see GetRandomEnemyIndex).
    public List<Object> Enemies;
    public Text WaveNumberText;
    public Statistics Statistics;

    // Read by AudioController: music is muted while waves are running.
    public static bool PlayMusic = false;

    // From this wave on, two enemies spawn at once: one in each half of the screen.
    private const int DoubleSpawnFromWave = 12;
    // Enemies always spawn just off the right edge of the screen.
    private const float SpawnX = 16.5f;
    private const float SpawnZ = -0.9f;

    private enum SpawnArea { Upper, Lower, Full }

    /// <summary>
    /// Vertical spawn limits per enemy, tuned by hand so each model stays fully on screen.
    /// Middle is where the screen is split into upper and lower halves for double spawns.
    /// </summary>
    private static readonly Dictionary<string, (float Bottom, float Middle, float Top)> SpawnHeights =
        new Dictionary<string, (float, float, float)>
        {
            [Tags.Missile] = (-4.19f, 0.48f, 5.15f),
            [Tags.GuidedMissile] = (-4.83f, -0.135f, 4.56f),
            [Tags.MultiMissile] = (-4.60f, -0.675f, 3.25f),
        };

    /// <summary>End-screen verdicts, checked from the top: the first entry whose wave is reached wins.</summary>
    private static readonly (int MinWave, string Verdict)[] Verdicts =
    {
        (18, "Unbelievable"),
        (15, "Insanely skilled"),
        (13, "Elite"),
        (11, "Kamikaze"),
        (9, "Journeyman"),
        (6, "Average Joe"),
        (3, "Babysitter"),
    };

    private GameManager gameManager;
    private Animator waveAnimator;
    private int waveNumber;

    void Start()
    {
        waveNumber = 1;
        waveAnimator = GameObject.FindGameObjectWithTag(Tags.Menu).GetComponent<Animator>();
        gameManager = GameObject.FindGameObjectWithTag(Tags.GameManager).GetComponent<GameManager>();
        ShowWaveBanner();
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        PlayMusic = false;
        yield return new WaitForSeconds(StartDelay);

        while (gameManager.IsPlayerAlive() && waveNumber <= MaxWaves)
        {
            for (var i = 0; i < GetNumberOfEnemiesInWave(); i++)
            {
                if (waveNumber >= DoubleSpawnFromWave)
                {
                    SpawnRandomEnemy(SpawnArea.Upper);
                    SpawnRandomEnemy(SpawnArea.Lower);
                }
                else
                {
                    SpawnRandomEnemy(SpawnArea.Full);
                }
                yield return new WaitForSeconds(GetSpawnDelay());
                if (!gameManager.IsPlayerAlive()) { break; }
            }
            if (!gameManager.IsPlayerAlive()) { break; }
            waveNumber++;
            ShowWaveBanner();
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

    private void ShowWaveBanner()
    {
        WaveNumberText.text = "WAVE " + waveNumber + "/" + MaxWaves;
        waveAnimator.SetTrigger("DisplayWave");
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

    private static GameObject FindGameOverPanel()
    {
        var menu = GameObject.FindGameObjectWithTag(Tags.Menu);
        return menu.transform.Find("GameOver").gameObject;
    }

    private void SetScore(bool survived)
    {
        var gameOver = FindGameOverPanel();
        gameOver.transform.Find("GameOverText").GetComponent<Text>().text = survived ? "Game Complete" : "Game Over";
        gameOver.transform.Find("GameOverScore").GetComponent<Text>().text = ResolveScore();
    }

    private string ResolveScore()
    {
        var enemiesKilled = Statistics.EnemiesSpawned - Statistics.EnemiesMissed - 1;
        enemiesKilled = Mathf.Floor(enemiesKilled < 1 ? 0 : enemiesKilled);
        // PlayerAttack counts single shots; the stats screen reports 3-shot bursts.
        var burstsFired = Mathf.Ceil(Statistics.BurstsFired / 3f);
        var accuracy = (enemiesKilled / (burstsFired < 1 ? 1 : burstsFired) * 100).ToString("F2");

        return "\nScore: " + Statistics.Score
            + "\nEnemies killed: " + enemiesKilled
            + "\nEnemies missed: " + Mathf.Floor(Statistics.EnemiesMissed)
            + "\nBursts fired: " + burstsFired
            + "\nAccuracy: " + accuracy + "%"
            + "\nDied at wave: " + waveNumber
            + "\n\nVerdict: " + Evaluate(waveNumber);
    }

    private static string Evaluate(int wave)
    {
        if (wave == 20)
        {
            return "GOD";
        }

        foreach (var (minWave, verdict) in Verdicts)
        {
            if (wave >= minWave)
            {
                return verdict;
            }
        }

        return "n00b";
    }

    private static void AnimateEndGame()
    {
        var gameOver = FindGameOverPanel();
        if (gameOver != null)
        {
            gameOver.SetActive(true);
            gameOver.transform.Find("RestartButton").gameObject.SetActive(true);
            var animator = gameOver.GetComponent<Animator>();
            animator.SetTrigger("EndGame");
        }
    }

    private void SpawnRandomEnemy(SpawnArea area)
    {
        var randomEnemy = Enemies[GetRandomEnemyIndex()];
        var position = new Vector3(SpawnX, GetRandomSpawnHeight(randomEnemy, area), SpawnZ);
        Statistics.EnemiesSpawned++;
        Instantiate(randomEnemy, position, Quaternion.identity);
    }

    private int GetNumberOfEnemiesInWave()
    {
        if (waveNumber == 20)
            return 200;
        if (waveNumber < 10)
            return 5;

        return waveNumber + 5;
    }

    /// <summary>Waves 1-2 only use the first enemy type, waves 3-5 the first two, wave 6+ all three.</summary>
    private int GetRandomEnemyIndex()
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
        // Integer division on purpose: the delay drops by one second every 10 waves.
        return SpawnDelayFactor - waveNumber / 10;
    }

    private static float GetRandomSpawnHeight(Object enemy, SpawnArea area)
    {
        if (!SpawnHeights.TryGetValue(((GameObject)enemy).tag, out var heights))
        {
            return Random.Range(-4f, 3f);
        }

        switch (area)
        {
            case SpawnArea.Upper:
                return Random.Range(heights.Middle, heights.Top);
            case SpawnArea.Lower:
                return Random.Range(heights.Bottom, heights.Middle);
            default:
                return Random.Range(heights.Bottom, heights.Top);
        }
    }
}
