using UnityEngine;
using System.Collections;

public class RUISKinectDisabler : MonoBehaviour {
    public void KinectNotConnected()
    {
        gameObject.SetActiveRecursively(false);
    }
}
