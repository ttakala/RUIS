using UnityEngine;
using System.Collections;

public class SpawnBowlingPins : MonoBehaviour {
    public GameObject bowlingPinsPrefab;
    public RUISPSMoveWand moveController;

    GameObject oldBowlingPins;
	
	void Update () {
        if (moveController.triangleButtonWasPressed)
        {
            if (oldBowlingPins)
            {
                Destroy(oldBowlingPins);
            }

            oldBowlingPins = Instantiate(bowlingPinsPrefab, transform.position, transform.rotation) as GameObject;
        }
	}
}
