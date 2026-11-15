using System;
using UnityEngine;

// A ScriptableObject event: one object raises it, and any number of objects listen.
// The sender and the listeners only know this asset. They do not know each other.
public abstract class EventChannel<T> : ScriptableObject
{
    public event Action<T> Raised;

    public void Raise(T value)
    {
        Raised?.Invoke(value);
    }
}
