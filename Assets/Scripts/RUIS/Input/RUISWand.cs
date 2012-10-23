using UnityEngine;
using System.Collections;

public abstract class RUISWand : MonoBehaviour {
    public abstract bool SelectionButtonWasPressed();
    public abstract bool SelectionButtonWasReleased();

    public abstract Vector3 GetAngularVelocity();

    public virtual Color color { get; set;} 
}
