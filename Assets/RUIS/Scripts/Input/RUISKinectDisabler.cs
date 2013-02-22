using UnityEngine;
using System.Collections;

public class RUISKinectDisabler : MonoBehaviour {
    public void KinectNotAvailable()
    {
        gameObject.SetActiveRecursively(false);

        RUISSkeletonWand[] skeletonWands = FindObjectsOfType(typeof(RUISSkeletonWand)) as RUISSkeletonWand[];
        foreach (RUISSkeletonWand wand in skeletonWands)
        {
            Debug.LogWarning("Disabling Skeleton Wand: " + wand.name);
            wand.gameObject.SetActiveRecursively(false);
        }
    }
}
