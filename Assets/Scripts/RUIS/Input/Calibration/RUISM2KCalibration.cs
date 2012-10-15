using UnityEngine;
using System.Collections.Generic;
using CSML;

public class RUISM2KCalibration : MonoBehaviour {
    public enum State
    {
        WaitingForSkeleton,
        WaitingForMovePress,
        Calibrating,
        CalibrationReview
    }

    private State currentState;
    private PSMoveWrapper psMoveWrapper;
    private NIPlayerManagerCOMSelection kinectSelection;
    private GUIText statusText;
    private GUITexture psEyeGUITexture;
    private Texture2D psEyeTexture;

    private int numberOfSamplesTaken;
    public int minNumberOfSamplesToTake = 50;
    private float timeSinceLastSample;
    public float samplesPerSecond = 1;
    private float timeBetweenSamples;

    private List<Vector3> rawPSMoveSamples;
    private List<Vector3> psMoveSamples;
    private List<Vector3> kinectSamples;

    private int calibratingPSMoveControllerId;

    public Transform rightHandVisualizer;

    public GameObject calibrationSphere;
    public GameObject calibrationCube;

    public RUISCoordinateSystem coordinateSystem;

    public RUISPSMoveController moveController;

    public string xmlFilename = "testm2k.xml";

    List<GameObject> calibrationSpheres;
    List<GameObject> moveCalibrationVisualizationCubes;

    private Vector3 floorNormal;

    private bool showDebugInfo = false;

    public GameObject calibrationGameObjects;
    public GameObject calibrationReviewGameObjects;

    float totalErrorDistance = 0;
    float averageError = 0;
    float errorStandardDeviation = 0;

    OpenNI.SceneAnalyzer sceneAnalyzer;

    public GameObject floorPlane;
    public GameObject kinectModelObject;


    void Awake () {
        currentState = State.WaitingForSkeleton;
        psMoveWrapper = GetComponent<PSMoveWrapper>();
        kinectSelection = GetComponent<NIPlayerManagerCOMSelection>();
        statusText = GetComponentInChildren<GUIText>();
        psEyeGUITexture = GetComponentInChildren<GUITexture>();

        rawPSMoveSamples = new List<Vector3>();
        psMoveSamples = new List<Vector3>();
        kinectSamples = new List<Vector3>();
	}

    void Start()
    {
        psMoveWrapper.Connect();

        Debug.Log("Connecting to PS Move");

        sceneAnalyzer = new OpenNI.SceneAnalyzer((FindObjectOfType(typeof(OpenNISettingsManager)) as OpenNISettingsManager).CurrentContext.BasicContext);
        sceneAnalyzer.StartGenerating();

        //moveController.gameObject.SetActiveRecursively(false);

        /*psEyeTexture = new Texture2D(640, 480, TextureFormat.ARGB32, false);

        psEyeGUITexture.texture = psEyeTexture;

        psMoveWrapper.CameraFrameResume();*/


        calibrationSpheres = new List<GameObject>();
        moveCalibrationVisualizationCubes = new List<GameObject>();

        SetCalibrationReviewShowing(false);

        /*List<Vector3> testList = new List<Vector3>();
        testList.Add(Vector3.right * 1.5f);
        testList.Add(new Vector3(0.5f, 1f, 0.5f));
        testList.Add(Vector3.forward * 2.0f);
        Orthonormalize(ref testList);
        foreach (Vector3 v in testList)
        {
            Debug.Log(v);
        }*/
    }

    void OnDestroy()
    {
        psMoveWrapper.Disconnect(false);
    }
	
	void Update () {
        /*Color32[] image = psMoveWrapper.GetCameraImage();
        if (image != null && image.Length == 640 * 480)
        {
            psEyeTexture.SetPixels32(image);
            psEyeTexture.Apply(false);
        }*/
        timeBetweenSamples = 1.0f / samplesPerSecond;

        switch (currentState)
        {
            case State.WaitingForSkeleton:
                DoWaitingForSkeleton();
                break;
            case State.WaitingForMovePress:
                DoWaitingForMovePress();
                break;
            case State.Calibrating:
                DoCalibrating();
                break;
            case State.CalibrationReview:
                DoCalibrationReview();
                break;
        }
	}

    private void DoWaitingForSkeleton()
    {
        statusText.text = "Step inside the area";
        if (kinectSelection.GetNumberOfSelectedPlayers() >= 1)
        {
            currentState = State.WaitingForMovePress;
        }
    }

    private void DoWaitingForMovePress()
    {
        statusText.text = "Take a Move controller into your right hand.\nPress X to start calibrating.\nPress Triangle to reset floor normal.";
        if (kinectSelection.GetNumberOfSelectedPlayers() < 1)
        {
            currentState = State.WaitingForSkeleton;
        }

        for (int i = 0; i < 4; i++)
        {
            if (psMoveWrapper.sphereVisible[i] && psMoveWrapper.isButtonCross[i])
            {
                currentState = State.Calibrating;
                numberOfSamplesTaken = 0;
                calibratingPSMoveControllerId = i;

                foreach (GameObject sphere in calibrationSpheres)
                {
                    Destroy(sphere);
                }

                calibrationSpheres = new List<GameObject>();

                rawPSMoveSamples = new List<Vector3>();
                psMoveSamples = new List<Vector3>();
                kinectSamples = new List<Vector3>();

                coordinateSystem.ResetTransforms();

                UpdateFloorNormal();
            }
            if (psMoveWrapper.sphereVisible[i] && psMoveWrapper.WasPressed(i, PSMoveWrapper.TRIANGLE))
            {
                UpdateFloorNormal();
            }
        }
    }


    private void DoCalibrating()
    {
        statusText.text = string.Format("Calibrating... {0}/{1} samples taken.", numberOfSamplesTaken, minNumberOfSamplesToTake);
        TakeSample();

        if (numberOfSamplesTaken >= minNumberOfSamplesToTake)
        {
            statusText.text = "All samples taken.\nProcessing...";
            currentState = State.CalibrationReview;
            
            CalculateTransformation();

            moveController.gameObject.SetActiveRecursively(true);


            float distance = 0;
            Vector3 error = Vector3.zero;
            List<float> errorMagnitudes = new List<float>();
            for (int i = 0; i < calibrationSpheres.Count; i++)
            {
                //Destroy(sphere);
                GameObject sphere = calibrationSpheres[i];
                Vector3 cubePosition = coordinateSystem.ConvertMovePosition(rawPSMoveSamples[i]);
                GameObject cube = Instantiate(calibrationCube, cubePosition, Quaternion.identity) as GameObject;
                cube.GetComponent<RUISMoveCalibrationVisualizer>().kinectCalibrationSphere = sphere;


                distance += Vector3.Distance(sphere.transform.position, cubePosition);
                errorMagnitudes.Add(distance);
                error += cubePosition - sphere.transform.position;

                sphere.transform.parent = calibrationReviewGameObjects.transform;
                cube.transform.parent = calibrationReviewGameObjects.transform;
            }

            totalErrorDistance = distance;
            averageError = distance / calibrationSpheres.Count;

            errorStandardDeviation = MathUtil.CalculateStandardDeviation(errorMagnitudes);

            SetCalibrationReviewShowing(true);
        }
    }

    private void DoCalibrationReview()
    {
        statusText.text = string.Format("Calibration finished!\n\nTotal Error: {0:0.####}\nMean: {1:0.####}\n", 
                totalErrorDistance, averageError);
    }

    private void TakeSample()
    {
        OpenNI.SkeletonJointPosition jointPosition;
        bool success = kinectSelection.GetPlayer(0).GetSkeletonJointPosition(OpenNI.SkeletonJoint.RightHand, out jointPosition);

        timeSinceLastSample += Time.deltaTime;

        //check if we should take sample
        if (!success || 
            !psMoveWrapper.sphereVisible[calibratingPSMoveControllerId] || 
            psMoveWrapper.handleVelocity[calibratingPSMoveControllerId].magnitude >= 10.0f||
            jointPosition.Confidence <= 0.5 ||
            timeSinceLastSample < timeBetweenSamples)
        {
            return;
        }

        rawPSMoveSamples.Add(psMoveWrapper.handlePosition[calibratingPSMoveControllerId]);
        psMoveSamples.Add(coordinateSystem.ConvertMovePosition(psMoveWrapper.handlePosition[calibratingPSMoveControllerId]));
        kinectSamples.Add(coordinateSystem.ConvertKinectPosition(jointPosition.Position));

        numberOfSamplesTaken++;
        timeSinceLastSample = 0;

        calibrationSpheres.Add(Instantiate(calibrationSphere, coordinateSystem.ConvertKinectPosition(jointPosition.Position), Quaternion.identity) as GameObject);
    }

    private void UpdateFloorNormal()
    {
        Debug.Log("Updating floor normal");
        coordinateSystem.ResetKinectFloorNormal();

        OpenNI.Plane3D floor = sceneAnalyzer.Floor;
        Vector3 newFloorNormal = new Vector3(floor.Normal.X, floor.Normal.Y, floor.Normal.Z).normalized;
        Vector3 newFloorPosition = coordinateSystem.ConvertKinectPosition(floor.Point);

        Debug.Log("normal: " + newFloorNormal);
        Debug.Log("position: " + newFloorPosition);
        
        /*OpenNI.SkeletonJointPosition torsoPosition;
        bool torsoSuccess = kinectSelection.GetPlayer(0).GetSkeletonJointPosition(OpenNI.SkeletonJoint.Torso, out torsoPosition);

        
        OpenNI.Point3D positionDifference = new OpenNI.Point3D(torsoPosition.Position.X - footPosition.Position.X,
                                                               torsoPosition.Position.Y - footPosition.Position.Y,
                                                               torsoPosition.Position.Z - footPosition.Position.Z);
        skeleton.transform.position = coordinateSystem.ConvertKinectPosition(floor.Point) + coordinateSystem.ConvertKinectPosition(positionDifference);
        */

        

        //Project the position of the kinect camera onto the floor
        //http://en.wikipedia.org/wiki/Point_on_plane_closest_to_origin
        //http://en.wikipedia.org/wiki/Plane_(geometry)
        float d = newFloorNormal.x * newFloorPosition.x + newFloorNormal.y * newFloorPosition.y + newFloorNormal.z * newFloorPosition.z;
        Vector3 closestFloorPointToKinect = new Vector3(newFloorNormal.x, newFloorNormal.y, newFloorNormal.z);
        closestFloorPointToKinect = (closestFloorPointToKinect * d) / closestFloorPointToKinect.sqrMagnitude;

        //transform the point from Kinect's coordinate system rotation to Unity's rotation
        closestFloorPointToKinect = Quaternion.FromToRotation(newFloorNormal, Vector3.up)  * closestFloorPointToKinect;
        //closestFloorPointToKinect = new Vector3(0, closestFloorPointToKinect.magnitude, 0);

        Debug.Log("closest: " + closestFloorPointToKinect);

        GameObject projectionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere) as GameObject;
        projectionSphere.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        projectionSphere.renderer.material.color = Color.red;
        projectionSphere.transform.position = closestFloorPointToKinect;

        floorPlane.transform.position = closestFloorPointToKinect;

        //show the tilt of the kinect camera on the kinect model
        kinectModelObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, newFloorNormal);

        coordinateSystem.SetKinectFloorNormal(newFloorNormal);
        floorNormal = newFloorNormal.normalized;
        coordinateSystem.SetKinectDistanceFromFloor(closestFloorPointToKinect.magnitude);
    }

    private void CalculateTransformation()
    {
        if (psMoveSamples.Count != numberOfSamplesTaken || kinectSamples.Count != numberOfSamplesTaken)
        {
            Debug.LogError("Mismatch in sample list lengths!");
        }

        int matrixSize = Mathf.Max(4, numberOfSamplesTaken); ;

        Matrix moveMatrix = Matrix.Zeros(psMoveSamples.Count, 4);
        for (int i = 1; i <= psMoveSamples.Count; i++)
        {
            moveMatrix[i, 1] = new Complex(psMoveSamples[i - 1].x);
            moveMatrix[i, 2] = new Complex(psMoveSamples[i - 1].y);
            moveMatrix[i, 3] = new Complex(psMoveSamples[i - 1].z);
            moveMatrix[i, 4] = new Complex(1.0f);
        }

        Matrix kinectMatrix = Matrix.Zeros(kinectSamples.Count, 3);
        for (int i = 1; i <= kinectSamples.Count; i++)
        {
            kinectMatrix[i, 1] = new Complex(kinectSamples[i - 1].x);
            kinectMatrix[i, 2] = new Complex(kinectSamples[i - 1].y);
            kinectMatrix[i, 3] = new Complex(kinectSamples[i - 1].z);
        }

        //perform a matrix solve Ax = B. We have to get transposes and inverses because moveMatrix isn't square
        //the solution is the same with (A^T)Ax = (A^T)B -> x = ((A^T)A)'(A^T)B
        Matrix transformMatrixSolution = (moveMatrix.Transpose() * moveMatrix).Inverse() * moveMatrix.Transpose() * kinectMatrix;

        Matrix error = moveMatrix * transformMatrixSolution - kinectMatrix;

        transformMatrixSolution = transformMatrixSolution.Transpose();

        Debug.Log(transformMatrixSolution);
        Debug.Log(error);

        List<Vector3> orthogonalVectors = MathUtil.Orthonormalize(
                                            MathUtil.ExtractRotationVectors(
                                                MathUtil.MatrixToMatrix4x4(transformMatrixSolution)
                                            )
                                          );
        Matrix4x4 rotationMatrix = CreateRotationMatrix(orthogonalVectors);
        Debug.Log(rotationMatrix);

        Matrix4x4 transformMatrix = MathUtil.MatrixToMatrix4x4(transformMatrixSolution);//CreateTransformMatrix(transformMatrixSolution);
        Debug.Log(transformMatrix);


        coordinateSystem.SetMoveToKinectTransforms(transformMatrix, rotationMatrix);
        coordinateSystem.SetKinectFloorNormal(floorNormal);
        coordinateSystem.SaveXML(xmlFilename);
    }

    

    private static Matrix4x4 CreateRotationMatrix(List<Vector3> vectors)
    {
        Matrix4x4 result = new Matrix4x4();
        result.SetColumn(0, new Vector4(vectors[0].x, vectors[0].y, vectors[0].z, 0));
        result.SetColumn(1, new Vector4(vectors[1].x, vectors[1].y, vectors[1].z, 0));
        result.SetColumn(2, new Vector4(vectors[2].x, vectors[2].y, vectors[2].z, 0));

        result[3, 3] = 1.0f;

        return result;
    }

    private static Matrix4x4 CreateTransformMatrix(Matrix transformMatrix)
    {
        Matrix4x4 result = new Matrix4x4();

        result.SetRow(0, new Vector4((float)transformMatrix[1, 1].Re, (float)transformMatrix[1, 2].Re, (float)transformMatrix[1, 3].Re, (float)transformMatrix[4, 1].Re));
        result.SetRow(1, new Vector4((float)transformMatrix[2, 1].Re, (float)transformMatrix[2, 2].Re, (float)transformMatrix[2, 3].Re, (float)transformMatrix[4, 2].Re));
        result.SetRow(2, new Vector4((float)transformMatrix[3, 1].Re, (float)transformMatrix[3, 2].Re, (float)transformMatrix[3, 3].Re, (float)transformMatrix[4, 3].Re));

        result.m33 = 1.0f;

        return result;
    }

    private void SetCalibrationReviewShowing(bool showing)
    {
        if (showing)
        {
            calibrationGameObjects.SetActiveRecursively(false);
            calibrationReviewGameObjects.SetActiveRecursively(true);
        }
        else
        {
            calibrationReviewGameObjects.SetActiveRecursively(false);
            calibrationGameObjects.SetActiveRecursively(true);
        }
    }
}
