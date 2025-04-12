using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Input/Input Reader", fileName = "InputReader")]
public class InputReader : ScriptableObject
{
    public event UnityAction<Vector2> MoveEvent;
    public event UnityAction BrakeEvent;
    public event UnityAction BrakeCanceledEvent;

    [SerializeField] private InputActionAsset _inputActions;

    private InputAction _moveInput;
    private InputAction _brakeInput;


    public void Init()
    {
        _moveInput = _inputActions.FindAction("Move");
        _brakeInput = _inputActions.FindAction("Brake");

        _moveInput.started += OnMove;
        _moveInput.performed += OnMove;
        _moveInput.canceled += OnMove;

        _brakeInput.started += OnBrake;
        _brakeInput.performed += OnBrake;
        _brakeInput.canceled += OnBrake;

        _moveInput.Enable();
        _brakeInput.Enable();
    }


    private void OnEnable()
    {
        Init();
    }


    private void OnDisable()
    {
        if(Application.isEditor) return;
        _moveInput.started -= OnMove;
        _moveInput.performed -= OnMove;
        _moveInput.canceled -= OnMove;

        _brakeInput.started -= OnBrake;
        _brakeInput.performed -= OnBrake;
        _brakeInput.canceled -= OnBrake;

        _moveInput.Disable();
        _brakeInput.Disable();
    }

    private void OnBrake(InputAction.CallbackContext context)
    {
        if(context.started) BrakeEvent?.Invoke();
        if(context.canceled) BrakeCanceledEvent?.Invoke();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }


}
