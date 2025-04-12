using UnityEngine;

public class FollowCar : MonoBehaviour
{
    [SerializeField] CarController _carController;
    [SerializeField] Transform _cameraPoint;

    Vector3 _velocity;
    private void FixedUpdate()
    {
        transform.LookAt(_carController.transform);

        transform.position = Vector3.SmoothDamp(transform.position, _cameraPoint.position, ref _velocity, 5f * Time.deltaTime);
    }
}
