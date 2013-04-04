using UnityEngine;
using System.Collections;
using System.Xml;

public class RUISKeystoningConfiguration : MonoBehaviour {
    RUISCamera ruisCamera;

    public RUISKeystoning.KeystoningCorners centerCameraCorners { get; private set; }
    public RUISKeystoning.KeystoningCorners leftCameraCorners { get; private set; }
    public RUISKeystoning.KeystoningCorners rightCameraCorners { get; private set; }

    RUISKeystoning.KeystoningCorners currentlyDragging = null;
    Camera cameraUnderModification = null;
    int draggedCornerIndex = -1;

    [HideInInspector]
    public bool drawKeystoningGrid = false;

    public RUISKeystoning.KeystoningSpecification centerCameraKeystoningSpec { get { return centerSpec; } }
    private RUISKeystoning.KeystoningSpecification centerSpec;

    public RUISKeystoning.KeystoningSpecification leftCameraKeystoningSpec { get { return leftSpec; } }
    private RUISKeystoning.KeystoningSpecification leftSpec;

    public RUISKeystoning.KeystoningSpecification rightCameraKeystoningSpec { get { return rightSpec; } }
    private RUISKeystoning.KeystoningSpecification rightSpec;

    public bool isEditing = false;

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
        if (!isEditing) return;

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

    public bool LoadFromXML(XmlDocument xmlDoc)
    {
        XmlNode centerCornerElement = xmlDoc.GetElementsByTagName("centerKeystone").Item(0);
        centerCameraCorners = new RUISKeystoning.KeystoningCorners(centerCornerElement);

        XmlNode leftCornerElement = xmlDoc.GetElementsByTagName("leftKeystone").Item(0);
        leftCameraCorners = new RUISKeystoning.KeystoningCorners(leftCornerElement);

        XmlNode rightCornerElement = xmlDoc.GetElementsByTagName("rightKeystone").Item(0);
        rightCameraCorners = new RUISKeystoning.KeystoningCorners(rightCornerElement);

        Optimize();

        return true;
    }

    public bool SaveToXML(XmlElement displayXmlElement)
    {
        XmlElement centerCornerElement = displayXmlElement.OwnerDocument.CreateElement("centerKeystone");
        centerCameraCorners.SaveToXML(centerCornerElement);
        displayXmlElement.AppendChild(centerCornerElement);

        XmlElement leftCornerElement = displayXmlElement.OwnerDocument.CreateElement("leftKeystone");
        leftCameraCorners.SaveToXML(leftCornerElement);
        displayXmlElement.AppendChild(leftCornerElement);

        XmlElement rightCornerElement = displayXmlElement.OwnerDocument.CreateElement("rightKeystone");
        rightCameraCorners.SaveToXML(rightCornerElement);
        displayXmlElement.AppendChild(rightCornerElement);

        return true;
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

    public void StartEditing()
    {
        isEditing = true;
    }

    public void EndEditing()
    {
        Optimize();
        isEditing = false;
        drawKeystoningGrid = false;
        ResetDrag();
    }
}
