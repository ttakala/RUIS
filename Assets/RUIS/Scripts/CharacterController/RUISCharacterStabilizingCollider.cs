using UnityEngine;
using System.Collections;

//assumes kinect ground is at Y = 0
[RequireComponent(typeof(CapsuleCollider))]
public class RUISCharacterStabilizingCollider : MonoBehaviour {
    public RUISPlainSkeletonController skeletonController;

    RUISSkeletonManager skeletonManager;
    int playerId;

    private CapsuleCollider collider;

    public float maxHeightChange = 5f;
    public float maxPositionChange = 10f;
    public float colliderRadiusTweaker = 1.5f;

	void Start () {
        skeletonManager = skeletonController.skeletonManager;
        playerId = skeletonController.playerId;

        collider = GetComponent<CapsuleCollider>();
	}
	
	void FixedUpdate () {
        Vector3 torsoPos = skeletonManager.skeletons[playerId].torso.position;
        Vector3 newPos = torsoPos;
        newPos.y = torsoPos.y / 2;

        collider.height = Mathf.Lerp(collider.height, torsoPos.y - collider.radius * colliderRadiusTweaker, maxHeightChange * Time.fixedDeltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, maxPositionChange * Time.fixedDeltaTime);
	}
}