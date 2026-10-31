using UnityEngine;

/// <summary>
/// Trigger volume around the play area: destroys anything that leaves it and
/// counts enemies that escaped past the player as missed.
/// </summary>
public class BorderManager : MonoBehaviour
{
    public Statistics Statistics;

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(Tags.GuidedMissile) || other.gameObject.CompareTag(Tags.Missile))
        {
            Statistics.EnemiesMissed += 1;
        }

        if (other.gameObject.CompareTag(Tags.MultiMissile))
        {
            // A MultiMissiles group has six missiles, so each one that escapes counts as 1/6 enemy.
            Statistics.EnemiesMissed += 1/6f;
            Destroy(other.transform.parent.gameObject);
        }
        Destroy(other.gameObject);
    }
}
