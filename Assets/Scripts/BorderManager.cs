using UnityEngine;

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
            Statistics.EnemiesMissed += 1/6f;
            Destroy(other.transform.parent.gameObject);
        }
        Destroy(other.gameObject);
    }
}
