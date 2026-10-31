using UnityEngine;

public class Impact : MonoBehaviour
{
    public GameObject ExplosionAnimation;
    public GameObject PlayerExplosionAnmiation;
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
            playerScore.SetPlayerScore(Score);
            var animationObject = Instantiate(ExplosionAnimation, other.transform.position, other.transform.rotation);
            Destroy(animationObject, 2f);
        }

        if (other.CompareTag(Tags.Player))
        {
            var animationObject = Instantiate(PlayerExplosionAnmiation, other.transform.position, other.transform.rotation);
            Destroy(animationObject, 2f);
            gameManager.KillPlayer();
        }

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
