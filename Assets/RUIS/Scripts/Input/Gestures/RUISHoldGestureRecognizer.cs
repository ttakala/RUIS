using UnityEngine;
using System.Collections;

public class RUISHoldGestureRecognizer : RUISGestureRecognizer
{
    float gestureProgress = 0;

    public float holdLength = 2.0f;
    public float speedThreshold = 0.25f;

    bool gestureStarted = false;
    float timeSinceStart;

    bool enabled = false;

    void Start()
    {
        ResetData();
    }

    void Update()
    {
        if (!enabled) return;

        if (gestureStarted && pointTracker.averageSpeed < speedThreshold)
        {
            timeSinceStart += Time.deltaTime;

            gestureProgress = Mathf.Clamp01(timeSinceStart / holdLength);
        }
        else if (pointTracker.averageSpeed < speedThreshold)
        {
            StartTiming();
        }
        else
        {
            ResetData();
        }
    }

    public override bool GestureTriggered()
    {
        return gestureProgress >= 0.99f;
    }

    public override float GetGestureProgress()
    {
        return gestureProgress;
    }

    public override void ResetProgress()
    {
        timeSinceStart = 0;
        gestureProgress = 0;
    }

    private void StartTiming()
    {
        ResetData();
        gestureStarted = true;
    }

    private void ResetData()
    {
        gestureStarted = false;
        gestureProgress = 0;
        timeSinceStart = 0;
    }

    public override void EnableGesture()
    {
        enabled = true;
        ResetData();
    }

    public override void DisableGesture()
    {
        enabled = false;
        ResetData();
    }
}
