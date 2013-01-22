using UnityEngine;
using System.Collections;

public class BallSpawner : MonoBehaviour {
    public GameObject ball;
    private PSMoveWrapper psMoveWrapper;

	void Awake () {
        psMoveWrapper = FindObjectOfType(typeof(PSMoveWrapper)) as PSMoveWrapper;	
	}
	
	void Update () {
	    for(int i = 0; i < 4; i++){
            if (psMoveWrapper.sphereVisible[i] && psMoveWrapper.WasPressed(i, PSMoveWrapper.CIRCLE))
            {
                Instantiate(ball, transform.position, transform.rotation);
            }
        }
	}
}
