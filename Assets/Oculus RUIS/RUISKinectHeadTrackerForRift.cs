using UnityEngine;
using System.Collections;

public class RUISKinectHeadTrackerForRift : MonoBehaviour {
    public int playerId = 0;

    public RUISSkeletonManager skeletonManager;

    void Awake()
    {
        if (!skeletonManager)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }
    }
	
	void Update () {
        if (skeletonManager.skeletons[playerId].head.positionConfidence == 1)
        {
            transform.localPosition = skeletonManager.skeletons[playerId].head.position;
        }
	}
}
