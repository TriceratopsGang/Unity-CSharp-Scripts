using UnityEngine;

public interface IVitals
{
    void ResetVitals();

    void SetHealth(float value);

    void TakeDamage(float amount, GameObject causer);
    void TakeHealing(float amount, GameObject causer);

    float GetHealthRatio();
}
