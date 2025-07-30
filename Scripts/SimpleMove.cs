using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 direction = Vector3.right;
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        if (direction == Vector3.zero || speed == 0f) return;

        transform.position += speed * Time.deltaTime * direction;
    }
}
