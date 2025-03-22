using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    [SerializeField] float _normalSpeed, _fastSpeed;
    [SerializeField] float _movementSpeed, _movementTime, _rotationAmout;
    [SerializeField] Vector3 _zoomAmount, _maxZoomAmount, _minZoomAmount;

    Vector3 _newPosition;
    Vector3 _newZoom;
    Quaternion _newRotation;

    [SerializeField] Camera _cam;

    Vector3 _dragStartPos, _dragCurrentPos;
    Vector3 _rotateStartPos, _rotateCurrentPos;

    private bool _isStatic = false;

    [SerializeField] private float offset = 2f;

    private Vector3 minBounds;
    private Vector3 maxBounds;

    public void SetIsStatic(bool value) => _isStatic = value;
    public void ResetCamera()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(new Vector3(0, 45, 0));
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _newPosition = transform.position;
        _newRotation = transform.rotation;
        _newZoom = _cam.transform.localPosition;
    }
    void Update()
    {
        if (_isStatic) return;
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

        _newZoom.y = Mathf.Clamp(_newZoom.y, _minZoomAmount.y, _maxZoomAmount.y);
        _newZoom.z = Mathf.Clamp(_newZoom.z, _maxZoomAmount.z, _minZoomAmount.z);

        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(_newPosition.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(_newPosition.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(_newPosition.z, minBounds.z, maxBounds.z)
        );

        transform.position = Vector3.Lerp(transform.position, clampedPosition, Time.deltaTime * _movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, _newRotation, Time.deltaTime * _movementTime);

        _cam.transform.localPosition = Vector3.Lerp(_cam.transform.localPosition, _newZoom, Time.deltaTime * _movementTime);


    }

    public void CalculateBounds(List<Vector3> points)
    {
        if (points == null || points.Count == 0)
        {
            return;
        }

        minBounds = points[0];
        maxBounds = points[0];

        foreach (var point in points)
        {
            minBounds.x = Mathf.Min(minBounds.x, point.x);
            minBounds.y = Mathf.Min(minBounds.y, point.y);
            minBounds.z = Mathf.Min(minBounds.z, point.z);

            maxBounds.x = Mathf.Max(maxBounds.x, point.x);
            maxBounds.y = Mathf.Max(maxBounds.y, point.y);
            maxBounds.z = Mathf.Max(maxBounds.z, point.z);
        }

        minBounds -= Vector3.one * offset;
        maxBounds += Vector3.one * offset;
    }

    void OnDrawGizmos()
    {
        if (minBounds != Vector3.zero && maxBounds != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y, minBounds.z), new Vector3(maxBounds.x, minBounds.y, minBounds.z));
            Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y, minBounds.z), new Vector3(minBounds.x, minBounds.y, maxBounds.z));
            Gizmos.DrawLine(new Vector3(maxBounds.x, minBounds.y, minBounds.z), new Vector3(maxBounds.x, minBounds.y, maxBounds.z));
            Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y, maxBounds.z), new Vector3(maxBounds.x, minBounds.y, maxBounds.z));

            Gizmos.DrawLine(new Vector3(minBounds.x, maxBounds.y, minBounds.z), new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
            Gizmos.DrawLine(new Vector3(minBounds.x, maxBounds.y, minBounds.z), new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
            Gizmos.DrawLine(new Vector3(maxBounds.x, maxBounds.y, minBounds.z), new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));
            Gizmos.DrawLine(new Vector3(minBounds.x, maxBounds.y, maxBounds.z), new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));

            Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y, minBounds.z), new Vector3(minBounds.x, maxBounds.y, minBounds.z));
            Gizmos.DrawLine(new Vector3(maxBounds.x, minBounds.y, minBounds.z), new Vector3(maxBounds.x, maxBounds.y, minBounds.z));
            Gizmos.DrawLine(new Vector3(minBounds.x, minBounds.y, maxBounds.z), new Vector3(minBounds.x, maxBounds.y, maxBounds.z));
            Gizmos.DrawLine(new Vector3(maxBounds.x, minBounds.y, maxBounds.z), new Vector3(maxBounds.x, maxBounds.y, maxBounds.z));
        }
    }
}
