using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RUISPointTracker))]
public abstract class RUISGestureRecognizer : MonoBehaviour {
    protected RUISPointTracker pointTracker;

	void Awake () {
        pointTracker = GetComponent<RUISPointTracker>();
	}

    public abstract bool GestureTriggered();
    public abstract float GetGestureProgress();
    public abstract void ResetProgress();

    public abstract void EnableGesture();
    public abstract void DisableGesture();
}
