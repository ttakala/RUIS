using UnityEngine;
using System.Collections;
using System.Xml;

public class RUISCamera : MonoBehaviour {
    [HideInInspector]
    public bool isHeadTracking;
    [HideInInspector]
    public bool isKeystoneCorrected;

    public Camera centerCamera; //the camera used for mono rendering
    public Camera leftCamera;
    public Camera rightCamera;
	public Camera keystoningCamera;

    public float eyeSeparation = 0.06f;
    public float zeroParallaxDistance = 0;

    [HideInInspector]
    public RUISDisplay associatedDisplay;

    private Rect normalizedScreenRect;
    private float aspectRatio;

    public bool isStereo { get { return associatedDisplay.isStereo; } }

    private bool oldStereoValue;
    private RUISDisplay.StereoType oldStereoTypeValue;

    RUISKeystoningConfiguration keystoningConfiguration;

    public float near = 0.3f;
    public float far = 1000;
    public float fieldOfView = 60;

    public bool copyHeadTrackerPosition = true;
    public RUISHeadTracker headTracker;
	public Vector3 KeystoningHeadTrackerPosition {
        get
        {
            return associatedDisplay.displayCenterPosition + associatedDisplay.DisplayNormal;
        }
	}
	
	public bool DEBUG = true;

    public void Awake()
    {
        keystoningConfiguration = GetComponent<RUISKeystoningConfiguration>();
    }

	public void Start () {
        if (!associatedDisplay)
        {
            Debug.LogError("Camera not associated to any display, disabling... " + name);
            gameObject.SetActiveRecursively(false);
            return;
        }

        centerCamera = camera;

        UpdateStereo();
        UpdateStereoType();

        if (!leftCamera || !rightCamera)
        {
            Debug.LogError("Cameras not set properly in RUISCamera: " + name);
        }

        SetupCameraTransforms();
		
		keystoningCamera.transform.position = KeystoningHeadTrackerPosition;
		keystoningCamera.gameObject.SetActiveRecursively(false);
	}
	
	public void Update () {
        

        if (oldStereoValue != associatedDisplay.isStereo)
        {
            UpdateStereo();
        }

        if (oldStereoTypeValue != associatedDisplay.stereoType)
        {
            UpdateStereoType();
        }

        
	}

    public void LateUpdate()
    {
		if (copyHeadTrackerPosition && headTracker != null)
        {
            transform.localPosition = headTracker.transform.position;
        }
		
		if(DEBUG)
		{
			centerCamera.fov = fieldOfView;
			leftCamera.fov = fieldOfView;
			rightCamera.fov = fieldOfView;
			
			centerCamera.near = near;
			leftCamera.near = near;
			rightCamera.near = near;
			
			centerCamera.far = far;
			leftCamera.far = far;
			rightCamera.far = far;
			
			
			centerCamera.ResetProjectionMatrix();
			leftCamera.ResetProjectionMatrix();
			rightCamera.ResetProjectionMatrix();
		} 
		else 
		{
		    //Matrix4x4[] projectionMatrices = GetProjectionMatricesWithoutKeystoning();
		    //centerCamera.projectionMatrix = projectionMatrices[0];
		    //leftCamera.projectionMatrix = projectionMatrices[1];
		    //rightCamera.projectionMatrix = projectionMatrices[2];

            centerCamera.projectionMatrix = CreateKeystoningObliqueFrustum();
            transform.position = KeystoningHeadTrackerPosition;

		    if (associatedDisplay.isKeystoneCorrected)
		    {
		        ApplyKeystoneCorrection();
		    }
		}
    }

    public Matrix4x4[] GetProjectionMatricesWithoutKeystoning()
    {
        if (associatedDisplay.isObliqueFrustum && headTracker)
        {
            return new Matrix4x4[] { CreateProjectionMatrix(headTracker.EyeCenterPosition), 
                                     CreateProjectionMatrix(headTracker.LeftEyePosition),
                                     CreateProjectionMatrix(headTracker.RightEyePosition) };
        }
        else
        {
            Matrix4x4 defaultMatrix = CreateDefaultFrustum();
            return new Matrix4x4[] { defaultMatrix, defaultMatrix, defaultMatrix };
        }
    }

    //http://csc.lsu.edu/~kooima/pdfs/gen-perspective.pdf
    //Generalized Perspective Projection
    //Robert Kooima
    public Matrix4x4 CreateProjectionMatrix(Vector3 trackerCoordinates)
    {
            Vector3 va = associatedDisplay.BottomLeftPosition - trackerCoordinates;
            Vector3 vb = associatedDisplay.BottomRightPosition - trackerCoordinates;
            Vector3 vc = associatedDisplay.TopLeftPosition - trackerCoordinates;
            Vector3 vr = associatedDisplay.DisplayRight;
            Vector3 vu = associatedDisplay.DisplayUp;
            Vector3 vn = associatedDisplay.DisplayNormal;

            float eyedistance = -(Vector3.Dot(va, vn));

            float left = (Vector3.Dot(vr, va) * near) / eyedistance;
            float right = (Vector3.Dot(vr, vb) * near) / eyedistance;
            float bottom = (Vector3.Dot(vu, va) * near) / eyedistance;
            float top = (Vector3.Dot(vu, vc) * near) / eyedistance;
            Matrix4x4 projectionMatrix = CreateFrustum(left, right, bottom, top, near, far);

            return projectionMatrix;
    }

    public Matrix4x4 CreateDefaultFrustum()
    {
        float right = -Mathf.Tan(fieldOfView / 2 * Mathf.Deg2Rad) * near;
        float left = -right;
        float top = right * aspectRatio;
        float bottom = -top;

        return CreateFrustum(right, left, top, bottom, near, far);
    }
	
	public Matrix4x4 CreateKeystoningObliqueFrustum(){
		return CreateProjectionMatrix (KeystoningHeadTrackerPosition);
	}

    private static Matrix4x4 CreateFrustum(float left, float right, float bottom, float top, float near, float far)
    {
        Matrix4x4 frustum = new Matrix4x4();

        frustum[0, 0] = 2 * near / (right - left);
        frustum[0, 2] = (right + left) / (right - left);
        frustum[1, 1] = 2 * near / (top - bottom);
        frustum[1, 2] = (top + bottom) / (top - bottom);
        frustum[2, 2] = -(far + near) / (far - near);
        frustum[2, 3] = -2 * far * near / (far - near);
        frustum[3, 2] = -1;

        return frustum;
    }

    public void SetupHeadTracking(RUISHeadTracker headTracker)
    {
        this.headTracker = headTracker;
        isHeadTracking = true;
    }

    public void SetupKeystoneCorrection()
    {
        isKeystoneCorrected = true;
    }

    private void ApplyHeadTrackingDistortion()
    {
    }

    private void ApplyKeystoneCorrection()
    {
        centerCamera.projectionMatrix *= keystoningConfiguration.centerCameraKeystoningSpec.GetMatrix();
        //Debug.Log(keystoningConfiguration.centerCameraKeystoningSpec.GetMatrix());
        leftCamera.projectionMatrix *= keystoningConfiguration.leftCameraKeystoningSpec.GetMatrix();
        rightCamera.projectionMatrix *= keystoningConfiguration.rightCameraKeystoningSpec.GetMatrix();
    }

    public void SetupCameraViewports(float relativeLeft, float relativeBottom, float relativeWidth, float relativeHeight, float aspectRatio)
    {
        normalizedScreenRect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
        this.aspectRatio = aspectRatio;

        centerCamera.rect = normalizedScreenRect;
        centerCamera.aspect = aspectRatio;

        if (associatedDisplay.stereoType == RUISDisplay.StereoType.SideBySide)
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth / 2, relativeHeight);
            rightCamera.rect = new Rect(relativeLeft + relativeWidth / 2, relativeBottom, relativeWidth / 2, relativeHeight);
        }
        else if (associatedDisplay.stereoType == RUISDisplay.StereoType.TopAndBottom)
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom + relativeHeight / 2, relativeWidth, relativeHeight / 2);
            rightCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight / 2);
        }
        else
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
            rightCamera.rect = new Rect(leftCamera.rect);
        }

        leftCamera.aspect = aspectRatio;
        rightCamera.aspect = aspectRatio;
    }

    public void SetupCameraTransforms()
    {
        float halfEyeSeparation = eyeSeparation / 2;
        leftCamera.transform.localPosition = new Vector3(-halfEyeSeparation, 0, 0);
        rightCamera.transform.localPosition = new Vector3(halfEyeSeparation, 0, 0);

        if (zeroParallaxDistance > 0)
        {
            float angle = Mathf.Acos(halfEyeSeparation / Mathf.Sqrt(Mathf.Pow(halfEyeSeparation, 2) + Mathf.Pow(zeroParallaxDistance, 2)));
            Vector3 rotation = new Vector3(0, angle, 0);
            rightCamera.transform.localRotation = Quaternion.Euler(-rotation);
            leftCamera.transform.localRotation = Quaternion.Euler(rotation);
        }
    }

    private void UpdateStereo()
    {
        if (associatedDisplay.isStereo)
        {
            centerCamera.enabled = false;
            leftCamera.enabled = true;
            rightCamera.enabled = true;
        }
        else
        {
            centerCamera.enabled = true;
            leftCamera.enabled = false;
            rightCamera.enabled = false;
        }

        oldStereoValue = associatedDisplay.isStereo;
    }

    private void UpdateStereoType()
    {
        SetupCameraViewports(normalizedScreenRect.xMin, normalizedScreenRect.yMin, normalizedScreenRect.width, normalizedScreenRect.height, aspectRatio);
        oldStereoTypeValue = associatedDisplay.stereoType;
    }

    public void LoadKeystoningFromXML(XmlDocument xmlDoc)
    {
        keystoningConfiguration.LoadFromXML(xmlDoc);
    }

    public void SaveKeystoningToXML(XmlElement displayXmlElement)
    {
        keystoningConfiguration.SaveToXML(displayXmlElement);
    }

    public void OnDrawGizmos()
    {
        /*if (!associatedDisplay) return;

        Color color = Gizmos.color;
        Gizmos.color = new Color(50, 50, 50);
        Gizmos.DrawLine(headTracker.transform.position, associatedDisplay.TopRightPosition);
        Gizmos.DrawLine(headTracker.transform.position, associatedDisplay.BottomRightPosition);
        Gizmos.DrawLine(headTracker.transform.position, associatedDisplay.BottomLeftPosition);
        Gizmos.DrawLine(headTracker.transform.position, associatedDisplay.TopLeftPosition);


        Matrix4x4 originalMatrix = Gizmos.matrix;
        Matrix4x4 rotationMatrix = new Matrix4x4();
        rotationMatrix.SetTRS(Vector3.zero, Quaternion.LookRotation(associatedDisplay.DisplayNormal, associatedDisplay.DisplayUp), Vector3.one);
        //Gizmos.matrix = rotationMatrix;

        Gizmos.DrawCube((associatedDisplay.TopRightPosition + associatedDisplay.BottomRightPosition) / 2, new Vector3(0.1f, associatedDisplay.TopRightPosition.y - associatedDisplay.BottomRightPosition.y, 0.1f));
        Gizmos.DrawCube((associatedDisplay.TopLeftPosition + associatedDisplay.BottomLeftPosition) / 2, new Vector3(0.1f, associatedDisplay.TopLeftPosition.y - associatedDisplay.BottomLeftPosition.y, 0.1f));
        Gizmos.DrawCube((associatedDisplay.TopRightPosition + associatedDisplay.TopLeftPosition) / 2, new Vector3(associatedDisplay.TopRightPosition.x - associatedDisplay.TopLeftPosition.x, 0.1f, 0.1f));
        Gizmos.DrawCube((associatedDisplay.BottomRightPosition + associatedDisplay.BottomLeftPosition) / 2, new Vector3(associatedDisplay.BottomRightPosition.x - associatedDisplay.BottomLeftPosition.x, 0.1f, 0.1f));
        
        Gizmos.color = color;
        Gizmos.matrix = originalMatrix;*/
    }
}
