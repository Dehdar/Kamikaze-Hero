using UnityEngine;

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
