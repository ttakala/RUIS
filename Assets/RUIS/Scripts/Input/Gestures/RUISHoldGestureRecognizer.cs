using UnityEngine;
using System.Collections;

public class RUISHoldGestureRecognizer : RUISGestureRecognizer {
    float gestureProgress = 0;

    public float holdLength = 2.0f;
    public float speedThreshold = 0.05f;

    bool gestureStarted = false;
    float timeSinceStart;

	void Start () {
	
	}
	
	void Update () {
        if (gestureStarted && pointTracker.averageSpeed < speedThreshold)
        {
            timeSinceStart += Time.deltaTime;

            gestureProgress = Mathf.Clamp01(timeSinceStart / holdLength);
        }
        else if (pointTracker.averageSpeed < speedThreshold)
        {
            Debug.Log("Starting Gesture!");
            gestureStarted = true;
            timeSinceStart = 0;
        }
        else
        {
            gestureStarted = false;
        }
    }

    public override bool GestureTriggered()
    {
        if (gestureProgress >= 1.0f)
        {
            Debug.Log("TRIGGERING!");
            StartCoroutine(ResetTriggerAtEndOfFrame());
            return true;
        }
        else return false;
    }

    public override float GetGestureProgress()
    {
        return gestureProgress;
    }

    bool alreadyResetting;
    private IEnumerator ResetTriggerAtEndOfFrame()
    {
        if (alreadyResetting)
            yield break;

        alreadyResetting = true;

        yield return new WaitForEndOfFrame();

        gestureStarted = false;
        gestureProgress = 0;
        timeSinceStart = 0;
        alreadyResetting = false;
    }
}
