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
		
		//keystoningCamera.transform.position = KeystoningHeadTrackerPosition;
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
            transform.localPosition = headTracker.EyeCenterPosition;
            //transform.localRotation = headTracker.rotation;
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
		    Matrix4x4[] projectionMatrices = GetProjectionMatricesWithoutKeystoning();
		    centerCamera.projectionMatrix = projectionMatrices[0];
		    leftCamera.projectionMatrix = projectionMatrices[1];
		    rightCamera.projectionMatrix = projectionMatrices[2];

            /*centerCamera.projectionMatrix = CreateKeystoningObliqueFrustum();
            transform.position = KeystoningHeadTrackerPosition;*/

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

            Matrix4x4 M = Matrix4x4.identity;
            M.SetColumn(0, vr);
            M.SetColumn(1, vu);
            M.SetColumn(2, vn);

            Matrix4x4 T = Matrix4x4.identity;
            T.SetColumn(3, trackerCoordinates);
            T[3, 3] = 1;

            return projectionMatrix;

        /*Vector3 camToDisplay = associatedDisplay.displayCenterPosition - trackerCoordinates;
        float dd = Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal);
        float frx = -Vector3.Dot(camToDisplay, associatedDisplay.DisplayRight) / dd;
        float fry = -Vector3.Dot(camToDisplay, associatedDisplay.DisplayUp) / dd;
        float scx = dd / (0.5f * associatedDisplay.width); // Metreissä
        float scy = dd / (0.5f * associatedDisplay.height); // Metreissä
        if (dd > 0)
            scy *= -1;

        Matrix4x4 B = Matrix4x4.identity;
        B[0, 0] = scx;
        B[1, 1] = scy;
        B[0, 2] = scx * frx;
        B[1, 2] = scy * fry;

        
        //Vector3 eyeProjWall = associatedDisplay.DisplayNormal * Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal);
        float eyeProjWallX = trackerCoordinates.x - associatedDisplay.DisplayNormal.x * (Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal)); // Tässä on jostain syystä miinusmerkki
        float eyeProjWallY = trackerCoordinates.y + associatedDisplay.DisplayNormal.y * (Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal));
        float eyeProjWallZ = trackerCoordinates.z + associatedDisplay.DisplayNormal.z * (Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal));


        Matrix4x4 C = Camera(trackerCoordinates.x, trackerCoordinates.y, trackerCoordinates.z, eyeProjWallX, eyeProjWallY, eyeProjWallZ, associatedDisplay.DisplayUp.x, associatedDisplay.DisplayUp.y, associatedDisplay.DisplayUp.z);

        return CreateDefaultFrustum() * B * C;*/
    }

    private Matrix4x4 Camera(float eyeX, float eyeY, float eyeZ, float centerX, float centerY, float centerZ, float upX, float upY, float upZ) {
         float z0 = eyeX - centerX;
         float z1 = eyeY - centerY;
         float z2 = eyeZ - centerZ;
         float mag = Mathf.Sqrt(z0*z0 + z1*z1 + z2*z2);

         if (mag != 0) {
         z0 /= mag;
         z1 /= mag;
         z2 /= mag;
         }

         float y0 = upX;
         float y1 = upY;
         float y2 = upZ;

         float x0 = y1*z2 - y2*z1;
         float x1 = -y0*z2 + y2*z0;
         float x2 = y0*z1 - y1*z0;

         y0 = z1*x2 - z2*x1;
         y1 = -z0*x2 + z2*x0;
         y2 = z0*x1 - z1*x0;

         mag = Mathf.Sqrt(x0*x0 + x1*x1 + x2*x2);
         if (mag != 0) {
         x0 /= mag;
         x1 /= mag;
         x2 /= mag;
         }

         mag = Mathf.Sqrt(y0*y0 + y1*y1 + y2*y2);
         if (mag != 0) {
             y0 /= mag; 
             y1 /= mag;
             y2 /= mag;
         }

         // just does an apply to the main matrix,
         // since that'll be copied out on endCamera
         Matrix4x4 camera = Matrix4x4.identity;
         camera[0, 0] = x0;
         camera[0, 1] = x1;
         camera[0, 2] = x2;
         camera[1, 0] = y0;
         camera[1, 1] = y1;
         camera[1, 2] = y2;
         camera[2, 0] = z0;
         camera[2, 1] = z1;
         camera[2, 2] = z2;

         Matrix4x4 translation = Matrix4x4.identity;
         translation[0, 3] = -eyeX;
         translation[1, 3] = -eyeY;
         translation[2, 3] = -eyeY;

         return camera * translation;
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
