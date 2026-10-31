using System.Linq;
using UnityEngine;

public class GuidedMissileForward : MonoBehaviour
{
    public int Speed;
    public int GuideSpeed;
    private Rigidbody missileRigidbody;
    private Rigidbody playerRigidbody;
    private float tolerence;
    private GameManager gameManager;
    private GameObject player;

    void Start()
    {
        tolerence = 0.1f;
        missileRigidbody = GetComponent<Rigidbody>();
        gameManager = (GameManager)GameObject.FindGameObjectWithTag("GameManager").GetComponent("GameManager");
        player = GameObject.FindGameObjectWithTag("Player");
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

        if (playerCoordinateY - tolerence > missileCoordinateY)
        {
            missileNewCoordinateY = 1;
        }

        if (playerCoordinateY + tolerence < missileCoordinateY)
        {
            missileNewCoordinateY = -1;
        }

        missileRigidbody.MovePosition(transform.position + new Vector3(-1*Speed, missileNewCoordinateY*GuideSpeed, 0)*Time.deltaTime);
    }
}
