using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RUISPointTracker))]
public class RUISJumpGestureRecognizer : RUISGestureRecognizer {
    public int playerId = 0;
    public float requiredUpwardVelocity = 1.0f;
    public float timeBetweenJumps = 1.0f;
    public float feetHeightThreshold = 0.1f;

    public enum State
    {
        WaitingForJump,
        Jumping,
        AfterJump
    }
    public State currentState { get; private set; }


    private float timeCounter = 0;    
    private bool gestureEnabled = true;

    
    public Vector3 leftFootHeight { get; private set; }
    public Vector3 rightFootHeight { get; private set; }

    private RUISSkeletonManager skeletonManager;
    private RUISPointTracker pointTracker;

    public void Awake()
    {
        pointTracker = GetComponent<RUISPointTracker>();
        skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        ResetProgress();
    }

    public void Update()
    {
        if (!gestureEnabled) return;

            switch (currentState)
            {
                case State.WaitingForJump:
                    DoWaitingForJump();
                    break;
                case State.Jumping:
                    DoJumping();
                    break;
                case State.AfterJump:
                    DoAfterJump();
                    break;
            }
    }

    public override bool GestureTriggered()
    {
        return gestureEnabled && currentState == State.Jumping;
    }

    public override float GetGestureProgress()
    {
        return (gestureEnabled && currentState == State.Jumping) ? 1 : 0;
    }

    public override void ResetProgress()
    {
        currentState = State.WaitingForJump;

        timeCounter = 0;
    }



    public override void EnableGesture()
    {
        gestureEnabled = true;
        ResetProgress();
    }

    public override void DisableGesture()
    {
        gestureEnabled = false;
    }

    private void DoJumping()
    {
        currentState = State.AfterJump;
    }

    private void DoAfterJump()
    {
        timeCounter += Time.deltaTime;

        if (timeCounter >= timeBetweenJumps)
        {
            ResetProgress();
            return;
        }
    }

    private void DoWaitingForJump()
    {
        leftFootHeight = skeletonManager.skeletons[playerId].leftFoot.position;
        rightFootHeight = skeletonManager.skeletons[playerId].rightFoot.position;

        if (leftFootHeight.y >= feetHeightThreshold && rightFootHeight.y >= feetHeightThreshold && pointTracker.averageVelocity.y >= requiredUpwardVelocity)
        {
            currentState = State.Jumping;
            timeCounter = 0;
            return;
        }
    }
}
