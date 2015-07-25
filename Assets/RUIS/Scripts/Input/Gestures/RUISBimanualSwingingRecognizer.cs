using UnityEngine;
using System.Collections;

public class RUISBimanualSwingingRecognizer : MonoBehaviour {

	// Skeleton ID
	public int skeletonID = 0;
	
	// Body tracking type
	public RUISSkeletonController.bodyTrackingDeviceType bodyTrackingType = RUISSkeletonController.bodyTrackingDeviceType.Kinect2;
	
	// Angle limits
	[Range(0, 180)][Header("")]	
	public float activationAreaAngleBuffer = 45;
	[Range(0, 180)]
	public float activationAreaParallelAngleBuffer = 60;
	
	[Range(0, 180)][Header("")]	
	public float walkActivationAngleHigh = 45;
	[Range(0, 180)]
	public float walkActivationAngleLow = 10;
	[Range(0, 180)]
	public float runActivationAngleHigh = 120;
	[Range(0, 180)]
	public float runActivationAngleLow = 90;
	
	
	
	private float walkAngleMax = 45 + 45;
	private float walkAngleMin = 10 - 45;
	private float runAngleMax = 120 + 45;
	private float runAngleMin = 90 - 45;
	
	// Confidence settings
	[Header("")]
	public float maxConfidenceValue = 100;
	public float minConfidenceValue = 50;
	public float confidenceIncrease = 10; // Per full swing
	public float confidenceDecrease = 4;  // Per 1s
	
	// Private variables
	private RUISSkeletonManager skeletonManager;
	private float	L_armAngle_in_zy_plane, R_armAngle_in_zy_plane, angleDifference_in_zy_plane, L_armAngle_in_average_plane, 
					R_armAngle_in_average_plane, angleDifference_in_average_plane, walkActivationConfidence, runActivationConfidence, 
					lastAngleDifference = 0, lastAngleAtPulse = 0, deltaAngleDifference;
	private bool	isWalking, isRunning, walkTriggerPulse, runTriggerPulse, walkConditionsMet, runConditionsMet;
	private int 	bodyTrackingDeviceID, movementDirection = 1;
	private Vector3 R_handVectorProjected_to_zy, R_HandPositionProjected_to_zy, R_elbowpositionProjected_to_zy,
					R_handVectorProjected_to_average_plane, R_HandPositionProjected_to_averagePlane, R_elbowpositionProjected_to_averagePlane,
	 				L_handVectorProjected_to_zy, L_HandPositionProjected_to_zy, L_elbowpositionProjected_to_zy,
					L_handVectorProjected_to_average_plane, L_HandPositionProjected_to_averagePlane, L_elbowpositionProjected_to_averagePlane,
					spineForward, vectorUp, vectorRight, averageVector, zyPlane, averagePlane, vectorForward;
	
	
	
	// Debug
	Vector3 debugPosition;
	
	void Start () 
	{
		skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
		if(!skeletonManager) Debug.LogError( "Could not find skeletonManager." );
		debugPosition = GameObject.Find ("handMovementAngleIndicator").transform.position;
	}
	
	
	void Update () 
	{
		
		updateArmAngles();	// Update arm angle global variables
		checkConditions();	// Check and set (walk/run)TriggerPulse & (walk/run)ConditionsMet
		
		// Update high and low angles dynamically
		updateAngleLowHigh(ref walkActivationAngleLow, ref walkActivationAngleHigh, walkAngleMin, walkAngleMax, isWalking);
		updateAngleLowHigh(ref runActivationAngleLow, ref runActivationAngleHigh, runAngleMin, runAngleMax, isRunning);
		
		runActivationAngleHigh = Mathf.Clamp(runActivationAngleHigh, runAngleMin, runAngleMax);
		runActivationAngleLow = Mathf.Clamp(runActivationAngleLow, runAngleMin, runAngleMax);
	
		// Handle movement pulses
		if(walkTriggerPulse || runTriggerPulse) movementDirection = -movementDirection;	
		
		handleConfidenceChange(ref walkActivationConfidence, walkConditionsMet);
		handleConfidenceChange(ref runActivationConfidence, runConditionsMet);
		
		// Keep track of walking/running status
		isWalking = (walkActivationConfidence > minConfidenceValue);
		isRunning = (runActivationConfidence > minConfidenceValue);
		
		// Decrement confidence over time
		if(walkActivationConfidence > 0) walkActivationConfidence -= Time.deltaTime * confidenceDecrease;
		if(runActivationConfidence > 0) runActivationConfidence -= Time.deltaTime * confidenceDecrease;
		
		debug();
	}
	
	public float getTargetVelocity() {
		return Mathf.Clamp01((walkActivationConfidence - minConfidenceValue) / (maxConfidenceValue - minConfidenceValue)); 
	}
	
	private void updateAngleLowHigh(ref float angleLow, ref float angleHigh, float angleMin, float angleMax, bool movementActive)
	{
		float buffer = activationAreaAngleBuffer;
		
		if(movementActive) 
			buffer = activationAreaAngleBuffer / 2;
		
		if(movementDirection == -1) {
			if(L_armAngle_in_zy_plane > angleHigh) {
				angleHigh = L_armAngle_in_zy_plane;
				angleLow = angleHigh - buffer;
				
			}
			if(R_armAngle_in_zy_plane < angleLow) {
				angleLow = R_armAngle_in_zy_plane;
				angleHigh = angleLow + buffer;
			}		
		}
		else {
			if(R_armAngle_in_zy_plane > angleHigh) {
				angleHigh = R_armAngle_in_zy_plane;
				angleLow = angleHigh - buffer;
			}
			if(L_armAngle_in_zy_plane < angleLow) {
				angleLow = L_armAngle_in_zy_plane;
				angleHigh = angleLow + buffer;
			}	
		}
		
		angleHigh = Mathf.Clamp(angleHigh, angleMin + buffer, angleMax);
		angleLow = Mathf.Clamp(angleLow, angleMin, angleMax - buffer);
	}
	
	private void checkConditions() 
	{
	
		// Check if arms within activation angles
		walkTriggerPulse =
							(
								movementDirection == 1 && 
								(R_armAngle_in_zy_plane > walkActivationAngleHigh) && 
								(L_armAngle_in_zy_plane < walkActivationAngleLow)
							)
							||
							(
								movementDirection == -1 && 
								(R_armAngle_in_zy_plane < walkActivationAngleLow) && 
								(L_armAngle_in_zy_plane > walkActivationAngleHigh)
							);
							
		runTriggerPulse =
							(
								movementDirection == 1 && 
								(R_armAngle_in_zy_plane > runActivationAngleHigh) && 
								(L_armAngle_in_zy_plane < runActivationAngleLow)
							)
							||
							(
								movementDirection == -1 && 
								(R_armAngle_in_zy_plane < runActivationAngleLow) && 
								(L_armAngle_in_zy_plane > runActivationAngleHigh)
							);					
							 
		
		// Check if arms are within activation areas
		walkConditionsMet =  (
								(
								(
								L_armAngle_in_zy_plane < walkAngleMax && 
								L_armAngle_in_zy_plane > walkAngleMin && 
								R_armAngle_in_zy_plane < walkAngleMax && 
								R_armAngle_in_zy_plane > walkAngleMin 
								)
								)
								&& this.angleDifference_in_average_plane < activationAreaParallelAngleBuffer
								);
			
		runConditionsMet =  (
								(
								(
								L_armAngle_in_zy_plane < runAngleMax && 
								L_armAngle_in_zy_plane > runAngleMin && 
								R_armAngle_in_zy_plane < runAngleMax && 
								R_armAngle_in_zy_plane > runAngleMin 
								)
								)
								&& this.angleDifference_in_average_plane < activationAreaParallelAngleBuffer
								);
		
		
			
		
	}
	
	private void handleConfidenceChange(ref float confidence, bool conditionsMet) {
		if(confidence < maxConfidenceValue && conditionsMet)
			confidence += (deltaAngleDifference / activationAreaAngleBuffer) * confidenceIncrease;
	}
	
	private void updateArmAngles() 
	{
		// Support live switching between sensors
		if(bodyTrackingType == RUISSkeletonController.bodyTrackingDeviceType.Kinect1) bodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
		if(bodyTrackingType == RUISSkeletonController.bodyTrackingDeviceType.Kinect2) bodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
		if(bodyTrackingType == RUISSkeletonController.bodyTrackingDeviceType.GenericMotionTracker) bodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
		
		// Create plane normal that is parallel to torso forward and up direction
		vectorRight = skeletonManager.skeletons[bodyTrackingDeviceID, skeletonID].leftHip.position - skeletonManager.skeletons[bodyTrackingDeviceID, skeletonID].rightHip.position;//upperTorsoPosition - rightShoulder;
		vectorForward = Vector3.Cross(Vector3.up, vectorRight).normalized;
		zyPlane = Vector3.Cross(vectorForward, Vector3.up).normalized;
		
		Vector3 R_handPosition, R_elbowposition;
		Vector3 L_handPosition, L_elbowposition;
		R_handPosition = skeletonManager.skeletons[bodyTrackingDeviceID, skeletonID].rightHand.position;
		L_handPosition = skeletonManager.skeletons[bodyTrackingDeviceID, skeletonID].leftHand.position;
		R_elbowposition = skeletonManager.skeletons[bodyTrackingDeviceID, skeletonID].rightElbow.position;
		L_elbowposition = skeletonManager.skeletons[bodyTrackingDeviceID, skeletonID].leftElbow.position;
		
		averageVector = (R_handPosition - L_elbowposition +  L_handPosition - R_elbowposition) / 2;
		averagePlane = Vector3.Cross(vectorRight, averageVector).normalized;
		
		// Right hand angles
		R_HandPositionProjected_to_zy = MathUtil.ProjectPointOnPlane(zyPlane, Vector3.zero, R_handPosition);
		R_HandPositionProjected_to_averagePlane = MathUtil.ProjectPointOnPlane(averagePlane, Vector3.zero, R_handPosition);
		R_elbowpositionProjected_to_zy = MathUtil.ProjectPointOnPlane(zyPlane, Vector3.zero, R_elbowposition);
		R_elbowpositionProjected_to_averagePlane = MathUtil.ProjectPointOnPlane(averagePlane, Vector3.zero, R_elbowposition);
		R_handVectorProjected_to_zy = R_HandPositionProjected_to_zy - R_elbowpositionProjected_to_zy;
		R_handVectorProjected_to_average_plane = R_HandPositionProjected_to_averagePlane - R_elbowpositionProjected_to_averagePlane;
		R_armAngle_in_zy_plane = calculateAngleBetweenVectors(Vector3.down, R_handVectorProjected_to_zy, zyPlane);
		R_armAngle_in_average_plane = calculateAngleBetweenVectors(vectorForward, R_handVectorProjected_to_average_plane, averagePlane);
		
		// Left hand angles
		L_HandPositionProjected_to_zy = MathUtil.ProjectPointOnPlane(zyPlane, Vector3.zero, L_handPosition);
		L_HandPositionProjected_to_averagePlane = MathUtil.ProjectPointOnPlane(averagePlane, Vector3.zero, L_handPosition);
		L_elbowpositionProjected_to_zy = MathUtil.ProjectPointOnPlane(zyPlane, Vector3.zero, L_elbowposition);
		L_elbowpositionProjected_to_averagePlane = MathUtil.ProjectPointOnPlane(averagePlane, Vector3.zero, L_elbowposition);
		L_handVectorProjected_to_zy = L_HandPositionProjected_to_zy - L_elbowpositionProjected_to_zy;
		L_handVectorProjected_to_average_plane = L_HandPositionProjected_to_averagePlane - L_elbowpositionProjected_to_averagePlane;
		L_armAngle_in_zy_plane = calculateAngleBetweenVectors(Vector3.down, L_handVectorProjected_to_zy, zyPlane);
		L_armAngle_in_average_plane = calculateAngleBetweenVectors(vectorForward, L_handVectorProjected_to_average_plane, averagePlane);
		
		angleDifference_in_average_plane = Mathf.Abs(R_armAngle_in_average_plane - L_armAngle_in_average_plane);
		angleDifference_in_zy_plane = Mathf.Abs(L_armAngle_in_zy_plane - lastAngleAtPulse);
		
		// Keep track of hand movement angle
		deltaAngleDifference = angleDifference_in_zy_plane - lastAngleDifference;
		if(deltaAngleDifference < 0) deltaAngleDifference = 0;
		else lastAngleDifference = angleDifference_in_zy_plane;
		
		if(walkTriggerPulse || runTriggerPulse) 
		{
			lastAngleDifference = 0; // Reset when direction changes
			lastAngleAtPulse = L_armAngle_in_zy_plane;
		}
		
	}
	
	/*
	*
	*	Utils
	*
	*/
	
	private float calculateAngleBetweenVectors(Vector3 vectorA, Vector3 vectorB, Vector3 planeNormal)
	{
		float angleDirection = Vector3.Dot(Vector3.Cross(vectorA, vectorB).normalized, planeNormal);
		float angleDifference = Vector3.Angle(vectorA , vectorB) * angleDirection;
		return angleDifference;
	}
	
	private void debug() {
		
	/*
	*
	* 	For debug
	*
	*/
//		if(walkTriggerPulse) {if(isWalking) print ("walking!"); else print("walk!");} 
//		if(runTriggerPulse) {if(isRunning) print ("running!");	else print("run!");} 
//		if(this.angleDifference_in_average_plane > activationAreaParallelAngleBuffer) {print ("Hands open too wide!");}
		//GameObject.Find("confidenceIncreaseIndicator").transform.localScale = new Vector3(0.1509386f, deltaAngleDifference / confidenceIncrease, 0.2423561f);
		GameObject.Find("walkConfidenceIndicator").transform.localScale = new Vector3(0.1509386f, walkActivationConfidence / maxConfidenceValue, 0.2423561f);
		GameObject.Find("runConfidenceIndicator").transform.localScale = new Vector3(0.1509386f, runActivationConfidence / maxConfidenceValue, 0.2423561f);
		
		if(isWalking) GameObject.Find("walkConfidenceIndicator").GetComponent<Renderer>().material.color = Color.red;
		else GameObject.Find("walkConfidenceIndicator").GetComponent<Renderer>().material.color = Color.white;
		
		if(isRunning) GameObject.Find("runConfidenceIndicator").GetComponent<Renderer>().material.color = Color.red;
		else GameObject.Find("runConfidenceIndicator").GetComponent<Renderer>().material.color = Color.white;
		
		// For debug
		//DrawPlane(Vector3.zero, zyPlane);
		//DrawPlane(Vector3.zero, averagePlane);
		
		DrawLine(Vector3.zero, R_handVectorProjected_to_zy, Color.magenta);
		DrawLine(Vector3.zero, L_handVectorProjected_to_zy, Color.magenta);
		
		
		DrawLine(Vector3.zero, Quaternion.AngleAxis(walkAngleMax, Vector3.left) * Vector3.down * 0.5f , Color.blue); 
		DrawLine(Vector3.zero, Quaternion.AngleAxis(walkAngleMin, Vector3.left) * Vector3.down * 0.5f , Color.blue); 
		DrawLine(Vector3.zero, Quaternion.AngleAxis(walkActivationAngleHigh, Vector3.left) * Vector3.down * 0.5f , Color.blue);
		DrawLine(Vector3.zero, Quaternion.AngleAxis(walkActivationAngleLow, Vector3.left) * Vector3.down * 0.5f , Color.blue);
		
		DrawLine(Vector3.zero, Quaternion.AngleAxis(runAngleMax, Vector3.left) * Vector3.down * 0.5f , Color.green); 
		DrawLine(Vector3.zero, Quaternion.AngleAxis(runAngleMin, Vector3.left) * Vector3.down * 0.5f , Color.green); 
		DrawLine(Vector3.zero, Quaternion.AngleAxis(runActivationAngleHigh, Vector3.left) * Vector3.down * 0.5f, Color.green);
		DrawLine(Vector3.zero, Quaternion.AngleAxis(runActivationAngleLow, Vector3.left) * Vector3.down * 0.5f, Color.green);
		
		//DrawLine(Vector3.zero, R_handVectorProjected_to_average_plane, Color.blue);
		//DrawLine(Vector3.zero, L_handVectorProjected_to_average_plane, Color.blue);
		//DrawLine(Vector3.zero, averageVector, Color.red);
	}
	
	// For debug : http://answers.unity3d.com/questions/467458/how-to-debug-drawing-plane.html
	private void DrawPlane(Vector3 position , Vector3 normal) 
	{
		
		position = position + debugPosition;//this.transform.position;
		normal = normal * 2;
		Vector3 v3;
		if (normal.normalized != Vector3.forward)
			v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
		else
			v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude;;
		Vector3 corner0 = position + v3;
		Vector3 corner2 = position - v3;
		
		Quaternion q = Quaternion.AngleAxis(90.0f, normal);
		v3 = q * v3;
		Vector3 corner1 = position + v3;
		Vector3 corner3 = position - v3;
		Debug.DrawLine(corner0, corner2, Color.green);
		Debug.DrawLine(corner1, corner3, Color.green);
		Debug.DrawLine(corner0, corner1, Color.green);
		Debug.DrawLine(corner1, corner2, Color.green);
		Debug.DrawLine(corner2, corner3, Color.green);
		Debug.DrawLine(corner3, corner0, Color.green);
		//Debug.DrawRay(position, normal, Color.magenta);
		
		if(movementDirection == 1) GameObject.Find("directionIndicator").GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
		if(movementDirection == -1) GameObject.Find("directionIndicator").GetComponent<Renderer>().material.SetColor("_Color", Color.red);
	}
	private void DrawLine(Vector3 start, Vector3 end, Color color) 
	{
//		start = this.transform.position + start;
//		end = this.transform.position + end;
		
		start = debugPosition + start;
		end = debugPosition + end;
	
		end.x = debugPosition.x + 0.2f;
		start.x = debugPosition.x + 0.2f;
		Debug.DrawLine(start, end, color);
	}
	
}
