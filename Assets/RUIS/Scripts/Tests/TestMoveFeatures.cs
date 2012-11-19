using UnityEngine;
using System.Collections;

public class TestMoveFeatures : MonoBehaviour {
    RUISPSMoveWand psMoveController;
    int rumbleAmount = 0;

    void Awake()
    {
        psMoveController = GetComponent<RUISPSMoveWand>();
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (psMoveController.crossButtonWasPressed)
        {
            psMoveController.Rumble(10, 2);
        }

        int newRumbleAmount = rumbleAmount;
        if (psMoveController.circleButtonWasPressed)
        {
            newRumbleAmount--;
        }
        if (psMoveController.triangleButtonWasPressed)
        {
            newRumbleAmount++;
        }
        newRumbleAmount = Mathf.Clamp(newRumbleAmount, 0, 19);

        if (newRumbleAmount != rumbleAmount)
        {
            psMoveController.RumbleOn(newRumbleAmount);
            rumbleAmount = newRumbleAmount;
        }
	}
}
