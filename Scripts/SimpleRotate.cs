using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 angle = Vector3.right;
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        if (angle == Vector3.zero || speed == 0f) return;

        transform.Rotate(speed * Time.deltaTime * angle);
    }
}
