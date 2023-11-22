using UnityEngine;

public class BorderManager : MonoBehaviour
{
    public Statistics Statistics;

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "GuidedMissile" || other.gameObject.tag == "Missile")
        {
            Statistics.EnemiesMissed += 1;
        }

        if (other.gameObject.tag == "MultiMissile")
        {
            Statistics.EnemiesMissed += 1/6f;
            Destroy(other.transform.parent.gameObject);
        }
        Destroy(other.gameObject);
    }
}
