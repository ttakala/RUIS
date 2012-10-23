using UnityEngine;
using System.Collections;

public class ReelInSelection : MonoBehaviour {
    public float reelSpeed = 1.0f;
    
    RUISPSMoveController psMoveController;
    RUISWandSelector wandSelector;

    RUISSelectable selection;

    bool wasClampedToCertainDistance;
    float distanceClampedTo;

    float currentDistance = 1;

	void Awake () {
        psMoveController = GetComponent<RUISPSMoveController>();
        wandSelector = GetComponent<RUISWandSelector>();
	}

    void Update () {
        if (!wandSelector.Selection && selection)
        {
            selection.clampToCertainDistance = wasClampedToCertainDistance;
            selection.distanceToClampTo = distanceClampedTo;
        }

        if (!wandSelector.Selection || wandSelector.Selection.positionSelectionGrabType != RUISSelectable.SelectionGrabType.AlongSelectionRay)
        {
            selection = null;
            return;
        }

        if(!selection){
            selection = wandSelector.Selection;
            
            wasClampedToCertainDistance = selection.clampToCertainDistance;
            selection.clampToCertainDistance = true;

            distanceClampedTo = selection.distanceToClampTo;

            currentDistance = 1;
        }

        float currentDistanceChange = ((1 - psMoveController.triggerValue) - currentDistance) * Time.deltaTime * reelSpeed;
        currentDistance += currentDistanceChange;
        selection.distanceToClampTo = currentDistance * selection.DistanceFromSelectionRayOrigin;
	}
}
