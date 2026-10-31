using UnityEngine;

/// <summary>
/// Holds whether the player is still alive; other scripts query it.
/// </summary>
public class GameManager : MonoBehaviour
{
    private bool isPlayerAlive;

    void Start()
    {
        isPlayerAlive = true;
    }

    public void KillPlayer()
    {
        isPlayerAlive = false;
    }

    public bool IsPlayerAlive()
    {
        return isPlayerAlive;
    }
}
