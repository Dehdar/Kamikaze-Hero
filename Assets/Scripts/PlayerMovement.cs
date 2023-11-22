using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
  public int MovementSpeed;
  public PlayerBoundary Boundary;
  public int TouchSensitivityX;
  public int TouchSensitivityY;
  private Rigidbody playerRigidbody;
  private FireController fireController;
  private KamikazeController kamikazeController;

  void Start ()
  {
    fireController = (FireController)GameObject.FindGameObjectWithTag("FireController").GetComponent("FireController");
    kamikazeController = (KamikazeController)GameObject.FindGameObjectWithTag("KamikazeController").GetComponent("KamikazeController");
    playerRigidbody = GetComponent<Rigidbody>();
  }

  void FixedUpdate()
  {
#if UNITY_EDITOR
    Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif

#if UNITY_ANDROID
    if (Input.touchCount == 3 ||
      (Input.touchCount == 2 && !fireController.IsFirePressed() && kamikazeController.IsKamikazeFireTouched()) ||
      (Input.touchCount == 2 && fireController.IsFirePressed() && !kamikazeController.IsKamikazeFireTouched()) ||
      (Input.touchCount == 1 && !fireController.IsFirePressed() && !kamikazeController.IsKamikazeFireTouched()))
    {
      var input = Input.GetTouch(fireController.IsFirstTouchFire ? 1 : 0).deltaPosition;
      Move(input.x / TouchSensitivityX, input.y / TouchSensitivityY);
    }
#endif
  }

  private void Move(float x, float y)
  {
    var newPosition = transform.position + new Vector3(x, y, 0) * MovementSpeed * Time.deltaTime;
    var clampedPosition = ClampPositionAgainstBoundaries(newPosition);
    playerRigidbody.MovePosition(clampedPosition);
  }

  private Vector3 ClampPositionAgainstBoundaries(Vector3 movement)
  {
    movement.x = Mathf.Clamp(movement.x, Boundary.HorizontalMin, Boundary.HorizontalMax);
    movement.y = Mathf.Clamp(movement.y, Boundary.VerticalMin, Boundary.VerticalMax);
    return movement;
  }
}