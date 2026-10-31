using UnityEngine;

/// <summary>
/// Enemy missile that flies left and steers vertically towards the player
/// until it passes the middle of the screen.
/// </summary>
public class GuidedMissileForward : MonoBehaviour
{
    public int Speed;
    public int GuideSpeed;
    private Rigidbody missileRigidbody;
    private Rigidbody playerRigidbody;
    // Vertical dead zone so the missile does not jitter once it is level with the player.
    private const float Tolerance = 0.1f;
    private GameManager gameManager;
    private GameObject player;

    void Start()
    {
        missileRigidbody = GetComponent<Rigidbody>();
        gameManager = GameObject.FindGameObjectWithTag(Tags.GameManager).GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag(Tags.Player);
        if (gameManager.IsPlayerAlive())
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        if (gameManager.IsPlayerAlive() && missileRigidbody.transform.position.x < 0f)
        {
            GuidedMovement();
        }
        else
        {
            ForwardMovement();
        }
    }

    private void ForwardMovement()
    {
        missileRigidbody.MovePosition(transform.position + Vector3.left * Speed * Time.deltaTime);
    }

    private void GuidedMovement()
    {
        var playerCoordinateY = playerRigidbody.transform.position.y;
        var missileCoordinateY = missileRigidbody.transform.position.y;
        var missileNewCoordinateY = 0f;

        if (playerCoordinateY - Tolerance > missileCoordinateY)
        {
            missileNewCoordinateY = 1;
        }

        if (playerCoordinateY + Tolerance < missileCoordinateY)
        {
            missileNewCoordinateY = -1;
        }

        missileRigidbody.MovePosition(transform.position + new Vector3(-1*Speed, missileNewCoordinateY*GuideSpeed, 0)*Time.deltaTime);
    }
}
