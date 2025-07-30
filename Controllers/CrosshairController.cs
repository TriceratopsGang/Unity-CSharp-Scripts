using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeReference] private CharacterController player;
    [SerializeReference] private Image[] lines;
    [SerializeReference] private Image centerDot;

    [Header("Settings")]
    [SerializeField] private Color color = Color.white;
    [SerializeField] private float minSize = 96f;
    [SerializeField] private float maxSize = 512f;
    [SerializeField] private float sizeScale = 0.1f;

    private Image[] allImages;
    private RectTransform rect;

    private void Awake()
    {
        allImages = GetComponentsInChildren<Image>();
        rect = GetComponent<RectTransform>();

        for (int i = 0; i < allImages.Length; i++) allImages[i].color = color;

        rect.sizeDelta = new Vector2(minSize, minSize);
    }

    private void Update()
    {
        if (player != null)
        {
            float speed = new Vector3(player.velocity.x, 0f, player.velocity.z).magnitude;

            float newSize = Mathf.Lerp(minSize, maxSize, speed * sizeScale);
            newSize = Mathf.Clamp(newSize, minSize, maxSize);

            rect.sizeDelta = new Vector2(newSize, newSize);
        }
    }
}
