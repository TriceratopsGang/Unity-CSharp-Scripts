using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthUpdatedEvent : UnityEvent<float, float> { }

public class VitalsComponent : MonoBehaviour, IVitals
{
    [Header("Health")]
    [SerializeField] private float _maxHealth = 100f;   //Also used as starting health

    public HealthUpdatedEvent OnHealthUpdated;           //Float = ratio & current.
    public UnityEvent<float> OnHealed;                  //Float = amount healed.
    public UnityEvent<float> OnDamaged;                 //Float = amount damaged.
    public UnityEvent OnDeath;

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    public bool IsDead => CurrentHealth <= 0f;
    public bool IsFullHealth => CurrentHealth >= MaxHealth;

    private bool CanBeDamaged => CurrentHealth > 0f;
    private bool CanBeHealed => CurrentHealth > 0f && CurrentHealth < MaxHealth;

    private void Awake()
    {
        MaxHealth = Mathf.Clamp(_maxHealth, 1f, 999f);
        SetHealth(MaxHealth);
    }

    #region IVitals

    public float GetHealthRatio()
    {
        return (MaxHealth != 0f) ? CurrentHealth / MaxHealth : 0f;
    }

    public void ResetVitals()
    {
        SetHealth(MaxHealth);
    }

    public void SetHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(value, 0f, MaxHealth);
        OnHealthUpdated?.Invoke(GetHealthRatio(), CurrentHealth);

        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    public void TakeDamage(float amount, GameObject causer)
    {
        if (amount > 0f && causer != null && CanBeDamaged)
        {
            SetHealth(CurrentHealth - amount);
        }
    }

    public void TakeHealing(float amount, GameObject causer)
    {
        if (amount > 0f && causer != null && CanBeHealed)
        {
            SetHealth(CurrentHealth + amount);
        }
    }

    #endregion
}
