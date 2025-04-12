using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] InputReader _inputReader;
    [SerializeField] WheelCollider _frontRightWheelCollider;
    [SerializeField] WheelCollider _frontLeftWheelCollider;
    [SerializeField] WheelCollider _backRightWheelCollider;
    [SerializeField] WheelCollider _backLeftWheelCollider;

    [SerializeField] Transform _frontRightWheelTransform, _frontLeftWheelTransform, _backRightWheelTransform, _backLeftWheelTransform;

    [SerializeField] Transform _carCenterOfMass;
    Rigidbody _rb;
    [SerializeField] float _motorForce = 100;
    [SerializeField] float _brakeForce = 45;
    [SerializeField] float _steerAngle = 45;

    bool _isBraking = false;
    Vector2 _moveInput;
    private void Awake()
    {
        _inputReader.Init();

        _inputReader.MoveEvent += OnMove;
        _inputReader.BrakeEvent += () => _isBraking = true;
        _inputReader.BrakeCanceledEvent += () => _isBraking = false;
    }

    private void StopBraking()
    {
        _frontRightWheelCollider.brakeTorque = 0;
        _frontLeftWheelCollider.brakeTorque = 0;
        _backRightWheelCollider.brakeTorque = 0;
        _backLeftWheelCollider.brakeTorque = 0;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.centerOfMass = _carCenterOfMass.position;
    }

    private void OnMove(Vector2 arg0)
    {
        _moveInput = arg0;
    }

    private void Update()
    {
        MotorForce();
        UpdateWheels();
        Steering();
        if(_isBraking) Brake();
        else StopBraking();
    }

    void Brake()
    {
        _frontRightWheelCollider.brakeTorque = _brakeForce;
        _frontLeftWheelCollider.brakeTorque = _brakeForce;
        _backRightWheelCollider.brakeTorque = _brakeForce;
        _backLeftWheelCollider.brakeTorque = _brakeForce;
    }

    void Steering()
    {
        _frontRightWheelCollider.steerAngle = _steerAngle * _moveInput.x;
        _frontLeftWheelCollider.steerAngle = _steerAngle * _moveInput.x;
    }

    void MotorForce()
    {
        _frontRightWheelCollider.motorTorque = _motorForce * _moveInput.y;
        _frontLeftWheelCollider.motorTorque = _motorForce * _moveInput.y;

        if(_moveInput.y == 0) { _backRightWheelCollider.motorTorque = 0; _backLeftWheelCollider.motorTorque = 0; }
    }

    void UpdateWheels()
    {
        RotateWheel(_frontRightWheelCollider, _frontRightWheelTransform);
        RotateWheel(_frontLeftWheelCollider, _frontLeftWheelTransform);
        RotateWheel(_backRightWheelCollider, _backRightWheelTransform);
        RotateWheel(_backLeftWheelCollider, _backLeftWheelTransform);
    }

    void RotateWheel(WheelCollider wheelCollider, Transform transform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        transform.position = pos;
        transform.rotation = rot;
    }
}
