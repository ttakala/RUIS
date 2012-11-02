using UnityEngine;
using System.Collections;

public abstract class CachedValue<T>
{
    private bool isValid = false;
    private T cachedValue;

    public void Invalidate()
    {
        isValid = false;
    }

    public T GetValue()
    {
        if (isValid) return cachedValue;

        cachedValue = CalculateValue();

        return cachedValue;
    }

    protected abstract T CalculateValue();
}
