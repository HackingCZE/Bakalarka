using UnityEngine;

public class SmoothRotateObj : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] Vector3 rotateDir;

    private void Update()
    {
        transform.Rotate(rotateDir * Time.deltaTime * speed);
    }
}
