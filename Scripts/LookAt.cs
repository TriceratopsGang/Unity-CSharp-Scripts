using UnityEngine;

public class LookAt : MonoBehaviour
{
    [Header("Settings")]
    [SerializeReference] private Transform target;
    [SerializeField] private bool smoothLook = false;
    [SerializeField] private float smoothSpeed = 5f;

    private void Update()
    {
        if (target == null) return;

        if (smoothLook)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.LookAt(target);
        }
    }
}
