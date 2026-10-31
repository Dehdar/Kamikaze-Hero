using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Put on enemies: handles being hit by a bullet (score + explosion) or hitting the player (game over).
/// </summary>
public class Impact : MonoBehaviour
{
    public GameObject ExplosionAnimation;
    // FormerlySerializedAs keeps prefabs saved with the old misspelled name working.
    [FormerlySerializedAs("PlayerExplosionAnmiation")]
    public GameObject PlayerExplosionAnimation;
    public int Score;
    private GameManager gameManager;
    private PlayerScore playerScore;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag(Tags.GameManager).GetComponent<GameManager>();
        if (gameManager.IsPlayerAlive())
        {
            playerScore = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<PlayerScore>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.Border) || other.CompareTag(Tags.Missile) || other.CompareTag(Tags.GuidedMissile) || other.CompareTag(Tags.MultiMissile))
        {
            return;
        }

        if (other.CompareTag(Tags.Bullet))
        {
            playerScore.AddScore(Score);
            var animationObject = Instantiate(ExplosionAnimation, other.transform.position, other.transform.rotation);
            Destroy(animationObject, 2f);
        }

        if (other.CompareTag(Tags.Player))
        {
            var animationObject = Instantiate(PlayerExplosionAnimation, other.transform.position, other.transform.rotation);
            Destroy(animationObject, 2f);
            gameManager.KillPlayer();
        }

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
