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
        gameManager = (GameManager)GameObject.FindGameObjectWithTag("GameManager").GetComponent("GameManager");
        if (gameManager.IsPlayerAlive())
        {
            playerScore = (PlayerScore)GameObject.FindGameObjectWithTag("Player").GetComponent("PlayerScore");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Border" || other.tag == "Missile" || other.tag == "GuidedMissile" || other.tag == "MultiMissile")
        {
            return;
        }

        if (other.tag == "Bullet")
        {
            playerScore.SetPlayerScore(Score);
            var animationObject = Instantiate(ExplosionAnimation, other.transform.position, other.transform.rotation);
            Destroy(animationObject, 2f);
        }

        if (other.tag == "Player")
        {
            var animationObject = Instantiate(PlayerExplosionAnmiation, other.transform.position, other.transform.rotation);
            Destroy(animationObject, 2f);
            gameManager.KillPlayer();
        }

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
