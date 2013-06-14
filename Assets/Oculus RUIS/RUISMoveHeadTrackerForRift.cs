using UnityEngine;
using System.Collections;

public class RUISMoveHeadTrackerForRift : MonoBehaviour
{
    public RUISPSMoveWand moveWand;

    void Update()
    {
        if (moveWand.moveButtonWasPressed)
        {
            OVRDevice.ResetOrientation(0);
        }

        transform.localPosition = moveWand.transform.position;
    }
}
