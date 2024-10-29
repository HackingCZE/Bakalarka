using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float _normalSpeed, _fastSpeed;
    [SerializeField] float _movementSpeed, _movementTime, _rotationAmout;
    [SerializeField] Vector3 _zoomAmount, _maxZoomAmount, _minZoomAmount;

    Vector3 _newPosition;
    Vector3 _newZoom;
    Quaternion _newRotation;

    [SerializeField] Camera _cam;

    Vector3 _dragStartPos, _dragCurrentPos;
    Vector3 _rotateStartPos, _rotateCurrentPos;

    private void Start()
    {
        _newPosition = transform.position;
        _newRotation = transform.rotation;
        _newZoom = _cam.transform.localPosition;
    }
    void Update()
    {
        HandleMovementInput();
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            _newZoom += Input.mouseScrollDelta.y * _zoomAmount;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                _dragStartPos = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                _dragCurrentPos = ray.GetPoint(entry);

                _newPosition = transform.position + _dragStartPos - _dragCurrentPos;
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            _rotateStartPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            _rotateCurrentPos = Input.mousePosition;

            Vector3 difference = _rotateStartPos - _rotateCurrentPos;

            _rotateStartPos = _rotateCurrentPos;

            _newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }

    void HandleMovementInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _movementSpeed = _fastSpeed;
        }
        else
        {
            _movementSpeed = _normalSpeed;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            _newPosition += (transform.forward * _movementSpeed);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            _newPosition += (transform.forward * -_movementSpeed);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            _newPosition += (transform.right * _movementSpeed);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            _newPosition += (transform.right * -_movementSpeed);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            _newRotation *= Quaternion.Euler(Vector3.up * _rotationAmout);
        }

        if (Input.GetKey(KeyCode.E))
        {
            _newRotation *= Quaternion.Euler(Vector3.up * -_rotationAmout);
        }

        if (Input.GetKey(KeyCode.R))
        {
            _newZoom += _zoomAmount;
        }

        if (Input.GetKey(KeyCode.F))
        {
            _newZoom -= _zoomAmount;
        }

        _newZoom.y = Mathf.Clamp(_newZoom.y, _minZoomAmount.y, _maxZoomAmount.y);
        _newZoom.z = Mathf.Clamp(_newZoom.z, _maxZoomAmount.z, _minZoomAmount.z);


        transform.position = Vector3.Lerp(transform.position, _newPosition, Time.deltaTime * _movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, _newRotation, Time.deltaTime * _movementTime);

        _cam.transform.localPosition = Vector3.Lerp(_cam.transform.localPosition, _newZoom, Time.deltaTime * _movementTime);


    }
}
