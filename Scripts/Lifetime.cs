using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(1f, 100f)] private float lifetime = 10f;

    private void Awake()
    {
        //Consider switching Destroy with SetActive.
        Destroy(gameObject, lifetime);
    }
}
