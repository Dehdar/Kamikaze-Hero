using UnityEngine;

public class KamikazeController : MonoBehaviour
{
    public bool IsKamikazeFire;
    private bool touched;

    public void OnPointerDown()
    {
        if (Input.touchCount == 1)
        {
            IsKamikazeFire = true;
        }
        touched = true;
    }

    public void OnPointerUp()
    {
        IsKamikazeFire = false;
        touched = false;
    }

    public bool IsKamikazeFireTouched()
    {
        return touched;
    }
}
