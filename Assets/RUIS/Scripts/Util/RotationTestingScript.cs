/*****************************************************************************

Content    :   An internal class used to test out different quaternion rotation modifications
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RotationTestingScript : MonoBehaviour 
{
    public int controllerId = 0;
    public int flipSignsId = 0;
//    public PSMoveWrapper psMoveWrapper;
	public GameObject visualizer;
	GameObject[] visualizers = new GameObject[24];

	public float xIncrement = 1;
	public float yIncrement = 1;


	public Transform inputPose1;
	public Transform inputPose2;

    private int t = 1;
    private int u = 1;
    private int v = 1;
    private int w = 1;

	void Start () 
	{
//        visualizers[0].transform.rotation = Quaternion.LookRotation(Vector3.up);
//        psMoveWrapper.Connect();

		for(int i = 0; i < 24; ++i)
		{
			int rowIndex = i % 4;
			int columnIndex = i / 4;
			visualizers[i] = Instantiate(visualizer, transform.position + new Vector3(rowIndex * xIncrement, columnIndex * yIncrement, 0), Quaternion.identity);
		}
	}

//    void OnDestroy()
//    {
//        psMoveWrapper.Disconnect(false);
//    }

    void Update()
    {
		if(Input.GetButtonDown("Fire1") /* psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.CROSS) */)
        {
			print("aa");
            flipSignsId++;
            if (flipSignsId >= 16)
            {
                flipSignsId = 0;
            }
        }


		Quaternion inputRotation = inputPose1.localRotation;

		inputRotation = QuaternionFromMatrix(Matrix4x4.TRS(Vector3.zero, inputRotation, Vector3.one));
//		print(Matrix4x4.TRS(Vector3.zero, inputRotation, Vector3.one));

        if (flipSignsId > 7)
            t = -1;
        switch (flipSignsId % 8)
        {
            case 0:
                u = 1; v = 1; w = 1;
                break;
            case 1:
                u = -1; v = 1; w = 1;
                break;
            case 2:
                u = 1; v = -1; w = 1;
                break;
            case 3:
                u = 1; v = 1; w = -1;
                break;
            case 4:
                u = -1; v = -1; w = 1;
                break;
            case 5:
                u = 1; v = -1; w = -1;
                break;
            case 6:
                u = -1; v = 1; w = -1;
                break;
            case 7:
                u = -1; v = -1; w = -1;
                break;
        }

		float a = t * inputRotation.w; //psMoveWrapper.qOrientation[controllerId].w;
		float b = u * inputRotation.x; //psMoveWrapper.qOrientation[controllerId].x;
		float c = v * inputRotation.y; //psMoveWrapper.qOrientation[controllerId].y;
		float d = w * inputRotation.z; //psMoveWrapper.qOrientation[controllerId].z;

        // Generate all 24 quaternion element order combinations
        for(int i = 0; i < 24; ++i)
        {
            Quaternion quat = new Quaternion();

            switch (i)
            {
                case 0:
                    quat.w = a; quat.x = b; quat.y = c; quat.z = d; break;
                case 1:
                    quat.w = d; quat.x = a; quat.y = b; quat.z = c; break;
                case 2:
                    quat.w = c; quat.x = d; quat.y = a; quat.z = b; break;
                case 3:
                    quat.w = b; quat.x = c; quat.y = d; quat.z = a; break;

                case 4:
                    quat.w = d; quat.x = c; quat.y = b; quat.z = a; break;
                case 5:
                    quat.w = a; quat.x = d; quat.y = c; quat.z = b; break;
                case 6:
                    quat.w = b; quat.x = a; quat.y = d; quat.z = c; break;
                case 7:
                    quat.w = c; quat.x = b; quat.y = a; quat.z = d; break;

                case 17:
                    quat.w = d; quat.x = b; quat.y = a; quat.z = c; break;
                case 18:
                    quat.w = d; quat.x = a; quat.y = c; quat.z = b; break;
                case 19:
                    quat.w = d; quat.x = b; quat.y = c; quat.z = a; break;
                case 20:
                    quat.w = d; quat.x = c; quat.y = a; quat.z = b; break;

                case 8:
                    quat.w = a; quat.x = d; quat.y = b; quat.z = c; break;
                case 9:
                    quat.w = a; quat.x = c; quat.y = d; quat.z = b; break;
                case 10:
                    quat.w = a; quat.x = b; quat.y = d; quat.z = c; break;
                case 21:
                    quat.w = a; quat.x = c; quat.y = b; quat.z = d; break;

                case 11:
                    quat.w = b; quat.x = d; quat.y = a; quat.z = c; break;
                case 12:
                    quat.w = b; quat.x = a; quat.y = c; quat.z = d; break;
                case 13:
                    quat.w = b; quat.x = c; quat.y = a; quat.z = d; break;
                case 22:
                    quat.w = b; quat.x = d; quat.y = c; quat.z = a; break;

                case 14:
                    quat.w = c; quat.x = d; quat.y = b; quat.z = a; break;
                case 15:
                    quat.w = c; quat.x = a; quat.y = d; quat.z = b; break;
                case 16:
                    quat.w = c; quat.x = a; quat.y = b; quat.z = d; break;
                case 23:
                    quat.w = c; quat.x = b; quat.y = d; quat.z = a; break;
            }

            visualizers[i].transform.rotation = quat;
        }
    }


	public Quaternion QuaternionFromMatrix(Matrix4x4 m)
	{
		// Source: http://answers.unity3d.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2;
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2;
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2;
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2;
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		return q;
	}
}
