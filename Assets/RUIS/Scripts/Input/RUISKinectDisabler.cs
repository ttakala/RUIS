using UnityEngine;
using System.Collections;

public class RUISKinectDisabler : MonoBehaviour {
    public void KinectNotAvailable()
    {
        gameObject.SetActiveRecursively(false);
    }
}
