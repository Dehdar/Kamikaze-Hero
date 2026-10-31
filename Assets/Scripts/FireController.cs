using UnityEngine;

public class FireController : MonoBehaviour
{
    public bool IsFirstTouchFire;
    private bool touched;

    public void OnPointerDown()
    {
        if (Input.touchCount == 1)
        {
            IsFirstTouchFire = true;
        }
        touched = true;
    }

    public void OnPointerUp()
    {
        IsFirstTouchFire = false;
        touched = false;
    }

    public bool IsFirePressed()
    {
        return touched;
    }
}
