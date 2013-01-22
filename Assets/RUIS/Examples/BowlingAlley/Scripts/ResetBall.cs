using UnityEngine;
using System.Collections;

public class ResetBall : MonoBehaviour {
    public RUISPSMoveWand moveController;
    public Transform ballResetSpot;

    private bool shouldResetBall = true;

    void FixedUpdate()
    {
        if (shouldResetBall || moveController.crossButtonWasPressed)
        {
            transform.position = ballResetSpot.transform.position;
            transform.rotation = ballResetSpot.transform.rotation;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            shouldResetBall = false;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        shouldResetBall = true;
    }
}
