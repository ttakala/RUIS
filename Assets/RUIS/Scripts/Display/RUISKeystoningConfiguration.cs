using UnityEngine;
using System.Collections;

public class RUISKeystoningConfiguration : MonoBehaviour {
    RUISCamera ruisCamera;

    public RUISKeystoning.KeystoningCorners centerCameraCorners { get; private set; }
    public RUISKeystoning.KeystoningCorners leftCameraCorners { get; private set; }
    public RUISKeystoning.KeystoningCorners rightCameraCorners { get; private set; }

    RUISKeystoning.KeystoningCorners currentlyDragging = null;
    Camera cameraUnderModification = null;
    int draggedCornerIndex = -1;

    public bool drawKeystoningGrid = false;

    public RUISKeystoning.KeystoningSpecification centerCameraKeystoningSpec { get { return centerSpec; } }
    private RUISKeystoning.KeystoningSpecification centerSpec;

    public RUISKeystoning.KeystoningSpecification leftCameraKeystoningSpec { get { return leftSpec; } }
    private RUISKeystoning.KeystoningSpecification leftSpec;

    public RUISKeystoning.KeystoningSpecification rightCameraKeystoningSpec { get { return rightSpec; } }
    private RUISKeystoning.KeystoningSpecification rightSpec;

	void Awake () {
        ruisCamera = GetComponent<RUISCamera>();

        centerCameraCorners = new RUISKeystoning.KeystoningCorners();
        leftCameraCorners = new RUISKeystoning.KeystoningCorners();
        rightCameraCorners = new RUISKeystoning.KeystoningCorners();

        centerSpec = new RUISKeystoning.KeystoningSpecification();
        leftSpec = new RUISKeystoning.KeystoningSpecification();
        rightSpec = new RUISKeystoning.KeystoningSpecification();
	}
	
	void Update () {
	    if(Input.GetMouseButtonDown(0)){
            //figure out if we should start dragging some corner
            Camera camUnderClick = ruisCamera.associatedDisplay.GetCameraForScreenPoint(Input.mousePosition);
            if (camUnderClick == ruisCamera.centerCamera)
            {
                currentlyDragging = centerCameraCorners;
                cameraUnderModification = camUnderClick;
                
            }
            else if (camUnderClick == ruisCamera.leftCamera)
            {
                currentlyDragging = leftCameraCorners;
                cameraUnderModification = camUnderClick;
            }
            else if (camUnderClick == ruisCamera.rightCamera)
            {
                currentlyDragging = rightCameraCorners;
                cameraUnderModification = camUnderClick;
            }

            if (currentlyDragging != null)
            {
                draggedCornerIndex = currentlyDragging.GetClosestCornerIndex(cameraUnderModification.ScreenToViewportPoint(Input.mousePosition));
            }

            if (draggedCornerIndex == -1)
            {
                ResetDrag();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ResetDrag();

            Optimize();
        }

        if (currentlyDragging != null)
        {
            Vector2 newPos = cameraUnderModification.ScreenToViewportPoint(Input.mousePosition);
            newPos.x = Mathf.Clamp01(newPos.x);
            newPos.y = Mathf.Clamp01(newPos.y);
            currentlyDragging[draggedCornerIndex] = newPos;

            Optimize();
        }
	}

    private void ResetDrag()
    {
        currentlyDragging = null;
        cameraUnderModification = null;
        draggedCornerIndex = -1;
    }

    private void Optimize()
    {
        centerSpec = RUISKeystoning.Optimize(ruisCamera.centerCamera, centerCameraCorners);
        leftSpec = RUISKeystoning.Optimize(ruisCamera.leftCamera, leftCameraCorners);
        rightSpec = RUISKeystoning.Optimize(ruisCamera.rightCamera, rightCameraCorners);
    }
}
