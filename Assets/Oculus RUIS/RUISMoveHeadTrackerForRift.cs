using UnityEngine;
using System.Collections;

public class RUISMoveHeadTrackerForRift : MonoBehaviour
{
    public RUISPSMoveWand moveWand;
    public OVRCameraController riftCamera;

    private Vector3 offset = Vector3.zero;

    void Update()
    {
        if (moveWand.moveButtonWasPressed)
        {
            //offset = moveWand.transform.position;
            //Quaternion oldOffset = Quaternion.identity;
            //riftCamera.GetOrientationOffset(ref oldOffset);
            /*Quaternion orientation = Quaternion.identity;
            riftCamera.GetCameraOrientation(ref orientation);
            riftCamera.SetOrientationOffset(orientation);*/
            OVRDevice.ResetOrientation(0);
        }

        //transform.localPosition = moveWand.handlePosition;
        transform.localPosition = moveWand.transform.position - offset;
    }
}
