/*****************************************************************************

Content    :   Class for simple Kalman filtering
Authors    :   Tuukka Takala, derived from Peter Abeles' EJML Kalman class
Copyright  :   Copyright 2018 Tuukka Takala. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System;
using CSML;

/**
 * A Kalman filter is implemented by calling the generalized operations.  Much of the excessive
 * memory creation/destruction has been reduced from the KalmanFilterSimple.  However, there
 * is still room for improvement by using specialized algorithms directly.  The price paid
 * for this better performance is the need to manually manage memory and the need to have a
 * better understanding for how each of the operations works.
 *
 * <p>
 * This is an interface for a discrete time Kalman filter with no control input:<br>
 * <br>
 * x<sub>k</sub> = F<sub>k</sub> x<sub>k-1</sub> + w<sub>k</sub><br>
 * z<sub>k</sub> = H<sub>k</sub> x<sub>k</sub> + v<sub>k</sub> <br>
 * <br>
 * w<sub>k</sub> ~ N(0,Q<sub>k</sub>)<br>
 * v<sub>k</sub> ~ N(0,R<sub>k</sub>)<br>
 * </p>
 * @author Peter Abeles
 * 
 * ported to C# by Tuukka Takala
 */
public class KalmanFilter
{
//	private bool threeByThree = false; // This would require an efficient 3-by-3 matrix implementation
	private bool fourByFour   = false;

    // kinematics description
    private Matrix F;
    private Matrix Q;
    private Matrix H;
	private Matrix R;

    // sytem state estimate
    private Matrix x;
    private Matrix P;

    // these are predeclared for efficency reasons
    private Matrix y, u, S, S_inv;
    private Matrix K;

	private Matrix4x4 F4, Q4, H4, R4, P4, S4, K4, T4, S_inv4;
	private Vector4 y4, u4, x4;

	private Double[] state;
	
	private double[] previousMeasurements;
	private double[] tempZ;

	public bool skipIdenticalMeasurements = false;
	public int identicalMeasurementsCap = 10;
	private int identicalMeasurementsCount = 0;

    /**
     * Specify the kinematics model of the Kalman filter with  
     * default identity matrix values. This must be called 
     * first before any other functions.
     */
    public void Initialize(int dimenX, int dimenZ)
    {
//		if(dimenX == 3 && dimenZ ==3)
//			threeByThree = true;
//		else
//			threeByThree = false;
		if(dimenX == dimenZ && dimenX <= 4)
		{
			fourByFour = true;
			F4 = Matrix4x4.identity;
			Q4 = Matrix4x4.identity; 
			H4 = Matrix4x4.identity;
			P4 = Matrix4x4.identity;
			R4 = Matrix4x4.zero;
			S4 = Matrix4x4.zero;
			K4 = Matrix4x4.zero;
			T4 = Matrix4x4.zero;
			y4 = Vector4.zero;
			u4 = Vector4.zero;
			x4 = Vector4.zero;
			S_inv4 = Matrix4x4.zero;
			if(dimenX < 2)
			{
				Q4.m11 = 0;
			}
			if(dimenX < 3)
			{
				Q4.m22 = 0;
			}
			if(dimenX < 4)
			{
				Q4.m33 = 0;
			}
		}
		else
			fourByFour = false;

		if(!fourByFour)
		{
			F = Matrix.Identity(dimenX);
			Q = Matrix.Identity(dimenX);
			H = Matrix.Identity(dimenZ);
			R = new Matrix(dimenZ,dimenZ);

			y = new Matrix(dimenZ,1);
			u = new Matrix(dimenZ,1);
			S = new Matrix(dimenZ,dimenZ);
			S_inv = new Matrix(dimenZ,dimenZ);
			K = new Matrix(dimenX,dimenZ);

			x = new Matrix(dimenX,1);

			P = Matrix.Identity(dimenX);
		}

		//x.zero(); // Should be zero already
		state = new Double[dimenX];
		for(int i = 0; i<state.Length; ++i)
			state[i] = 0;

		previousMeasurements = new double[dimenZ];
		for(int i = 0; i<previousMeasurements.Length; ++i)
			previousMeasurements[i] = 0;
    }

    /**
     * Specify the kinematics model of the Kalman filter.  This must be called
     * first before any other functions.
     *
     * @param F State transition matrix.
     * @param Q process noise covariance.
     * @param H measurement projection matrix.
     */
    public void Initialize(Matrix F, Matrix Q, Matrix H) 
    {
	    int dimenX = F.ColumnCount;
	    int dimenZ = H.RowCount;

    	Initialize(dimenX, dimenZ);
    		
        this.F = F;
        this.Q = Q;
        this.H = H;
    }

	public void Reset()
	{
		Initialize(state.Length, y.RowCount);
	}
	
    /**
     * The prior state estimate and covariance.
     *
     * @param x The estimated system state.
     * @param P The covariance of the estimated system state.
     */
    public void SetState(Matrix x, Matrix P) 
    {
		this.x = x.Extract(1, x.RowCount, 1, x.ColumnCount); // this.x.set(x);
		this.P = P.Extract(1, P.RowCount, 1, P.ColumnCount); // this.P.set(P);
    }

	public void SetState(double[] newState)
	{
		if(newState != null)
		{
			for(int i=0; i < newState.Length && i < state.Length; ++i)
				state[i] = newState[i];
		}
	}

    public void SetQ(Matrix Q) 
    {
    	this.Q = Q.Extract(1, Q.RowCount, 1, Q.ColumnCount); // this.Q = Q;
    }

    public void SetQ(double q) 
    {
    	Q = Matrix.Identity(Q.RowCount);
		Q = Q * q; // scale(q, Q);
    }

    public void SetR(Matrix R) 
    {
    	this.R = R.Extract(1, R.RowCount, 1, R.ColumnCount); // this.S = R;
    }

    public void SetR(double r) 
    {
		if(fourByFour)
		{
			R4.m00 = (float) r;
			R4.m11 = (state.Length > 1)? (float) r : 0;
			R4.m22 = (state.Length > 2)? (float) r : 0;
			R4.m33 = (state.Length > 3)? (float) r : 0;
			R4.m01 = 0;
			R4.m02 = 0;
			R4.m03 = 0;
			R4.m10 = 0;
			R4.m12 = 0;
			R4.m13 = 0;
			R4.m20 = 0;
			R4.m21 = 0;
			R4.m23 = 0;
			R4.m30 = 0;
			R4.m31 = 0;
			R4.m32 = 0;
		}
		else
		{
	    	R = Matrix.Identity(R.RowCount);
			R = R * r; //scale(r, S);
		}
    }

	public void SetRDiagonal(double r) 
	{
		int rows 	= R.RowCount;
		int columns = R.ColumnCount;

		for(int i = 1; i <= rows; ++i)
			for(int j = 1; j <= columns; ++j)
				R[i, j].Re = r;
	}

    public void SetR(int row, int col, double r) 
    {
		R[row, col] = new Complex(r);
    	//this.S.set(row, col, r);
    }
    
    /**
     * Predicts the state of the system forward one time step.
     */
    public void Predict() 
	{
		if(fourByFour)
		{
			x4 = F4 * x4;
			P4 = ((F4*P4)*F4.transpose);
			AddMatrix4x4(ref P4, ref Q4, ref P4);
		}
		else
		{
	        // x = F x
			x = F*x;
			x = x.Re();
	        //mult(F,x,a);
	        //x.set(a);

	        // P = F P F' + Q
			
	        P = ((F*P)*F.Transpose()) + Q;
			P = P.Re();
			//mult(F,P,b);
	        //multTransB(b,F, P);
	        //addEquals(P,Q);
		}
    }

    /**
     * Updates the state provided the observation from a sensor.
     *
     * @param z Measurement.
     * @param R Measurement covariance.
     */
    public void Update(Matrix z, Matrix R) 
	{
		if(skipIdenticalMeasurements)
		{
			bool areIdentical = true;
			
			for(int i = 0; i<z.RowCount; ++i)
			{
				if(z[i+1, 1].Re != previousMeasurements[i])
				{
					areIdentical = false;
					previousMeasurements[i] = z[i+1, 1].Re;
				}
			}
			
			if(areIdentical && identicalMeasurementsCount < identicalMeasurementsCap)
			{
				++identicalMeasurementsCount;
				return;
			}
			else
				identicalMeasurementsCount = 0;
		}
		
        // y = z - H x
        //mult(H,x,y);
        //sub(z,y,y);
		y = z - (H*x);
		y = y.Re();

        // S = H P H' + R
        //mult(H,P,c);
        //multTransB(c,H,S);
        //addEquals(S,R);
		S = ((H*P)*H.Transpose()) + R;
		S = S.Re();

        // K = PH'S^(-1)
        //if( !invert(S,S_inv) ) throw new RuntimeException("Invert failed");
        //multTransA(H,S_inv,d);
        //mult(P,d,K);
		try
		{
			S_inv = S.Inverse();
		}
		catch(Exception e)
		{
			Debug.Log("KalmanFilter.cs: Failed to invert S matrix.\n" + e);
		}
		K = P*(H.Transpose()*S_inv);
		K = K.Re();

        // x = x + Ky
        //mult(K,y,a);
        //addEquals(x,a);
		x = x + (K*y);

        // P = (I-kH)P = P - (KH)P = P-K(HP)
        //mult(H,P,c);
        //mult(K,c,b);
        //subEquals(P,b);
		P = P - (K*(H*P));
		P = P.Re();
    }


    /**
     * Updates the state provided the observation from a sensor.
     *
     * @param z Measurement.
     */
    public void Update(double[] z) 
    {
		
		if(skipIdenticalMeasurements)
		{
			bool areIdentical = true;
			
			for(int i = 0; i<z.Length; ++i)
			{
				if(z[i] != previousMeasurements[i])
				{
					areIdentical = false;
					previousMeasurements[i] = z[i];
				}
			}
			
			if(areIdentical && identicalMeasurementsCount < identicalMeasurementsCap)
			{
				++identicalMeasurementsCount;
				return;
			}
			else
				identicalMeasurementsCount = 0;
		}


		if(fourByFour)
		{
			for(int i = 0; i<z.Length; ++i)
				u4[i] = (float) z[i];
			y4 = u4 - H4*x4;
			S4 = ((H4*P4)*H4.transpose);
			AddMatrix4x4(ref S4, ref R4, ref S4);

			if(state.Length == 4)
				S_inv4 = S4.inverse;
			else if(state.Length == 3)
				InverseMatrix3x3(ref S4, ref S_inv4);
			else if(state.Length == 2)
				InverseMatrix2x2(ref S4, ref S_inv4);
			else
				S_inv4.m00 = 1.0f / S4.m00;
			
			K4 = P4*(H4.transpose*S_inv4);
			x4 = x4 + (K4*y4);
			T4 = K4*(H4*P4);
			SubtractMatrix4x4(ref P4, ref T4, ref P4);

//			if(state.Length == 3)
//				Debug.Log(S4);
		}
		else
		{
	        // y = z - H x
	        //mult(H,x,y);
	        //for(int i=0; i<u.numRows; ++i)
	        //	u.set(i, (double) z[i]);
	        //sub(u,y,y);
			u = new Matrix(z);
			y = u - (H*x);
			y = y.Re();

	        // S = H P H' + R
	        //S_inv=S.copy();
	        //mult(H,P,c);
	        //multTransB(c,H,S);
	        //addEquals(S,S_inv);
			S = ((H*P)*H.Transpose()) + R;
			S = S.Re();

	        // K = PH'S^(-1)
	        //if( !invert(S,S_inv) ) throw new RuntimeException("Invert failed");
	        //multTransA(H,S_inv,d);
	        //mult(P,d,K);
			try
			{
				S_inv = S.Inverse();
			}
			catch(Exception e)
			{
				Debug.Log("KalmanFilter.cs: Failed to invert S matrix.\n" + e);
			}
			K = P*(H.Transpose()*S_inv);
			K = K.Re();

	        // x = x + Ky
	        //mult(K,y,a);
	        //addEquals(x,a);
			x = x + (K*y);

	        // P = (I-kH)P = P - (KH)P = P-K(HP)
	        //mult(H,P,c);
	        //mult(K,c,b);
	        //subEquals(P,b);
			P = P - (K*(H*P));
			P = P.Re();
		}
    }
    
    /**
     * Returns the current estimated state of the system.
     *
     * @return The state.
     */
    public Double[] GetState() 
    {
		if(fourByFour)
		{
			for(int i = 0; i<state.Length; ++i)
				state[i] = x4[i];

//			if(state.Length == 3)
//				Debug.Log(x4.x + " " + x4.y + " " + x4.z + " " + x4.w);
		}
		else
		{
			for(int i = 0; i<state.Length; ++i)
		  		state[i] = x[i+1, 1].Re;
		}
        return state;
    }

    /**
     * Returns the estimated state's covariance matrix.
     *
     * @return The covariance.
     */
    public Matrix GetCovariance() 
    {
        return P;
    }

	private void AddMatrix4x4(ref Matrix4x4 A, ref Matrix4x4 B, ref Matrix4x4 result)
	{
		result.m00 = A.m00 + B.m00;
		result.m01 = A.m01 + B.m01;
		result.m02 = A.m02 + B.m02;
		result.m03 = A.m03 + B.m03;

		result.m10 = A.m10 + B.m10;
		result.m11 = A.m11 + B.m11;
		result.m12 = A.m12 + B.m12;
		result.m13 = A.m13 + B.m13;

		result.m20 = A.m20 + B.m20;
		result.m21 = A.m21 + B.m21;
		result.m22 = A.m22 + B.m22;
		result.m23 = A.m23 + B.m23;

		result.m30 = A.m30 + B.m30;
		result.m31 = A.m31 + B.m31;
		result.m32 = A.m32 + B.m32;
		result.m33 = A.m33 + B.m33;
	}

	private void SubtractMatrix4x4(ref Matrix4x4 A, ref Matrix4x4 B, ref Matrix4x4 result)
	{
		result.m00 = A.m00 - B.m00;
		result.m01 = A.m01 - B.m01;
		result.m02 = A.m02 - B.m02;
		result.m03 = A.m03 - B.m03;

		result.m10 = A.m10 - B.m10;
		result.m11 = A.m11 - B.m11;
		result.m12 = A.m12 - B.m12;
		result.m13 = A.m13 - B.m13;

		result.m20 = A.m20 - B.m20;
		result.m21 = A.m21 - B.m21;
		result.m22 = A.m22 - B.m22;
		result.m23 = A.m23 - B.m23;

		result.m30 = A.m30 - B.m30;
		result.m31 = A.m31 - B.m31;
		result.m32 = A.m32 - B.m32;
		result.m33 = A.m33 - B.m33;
	}

	private void InverseMatrix3x3(ref Matrix4x4 m, ref Matrix4x4 result)
	{
		// computes the inverse of a matrix m
		float det = m.m00 * (m.m11 * m.m22 - m.m21 * m.m12) -
					m.m01 * (m.m10 * m.m22 - m.m12 * m.m20) +
					m.m02 * (m.m10 * m.m21 - m.m11 * m.m20);

		float invdet = 1.0f / det;

		// inverse of matrix m
		result.m00 = (m.m11 * m.m22 - m.m21 * m.m12) * invdet;
		result.m01 = (m.m02 * m.m21 - m.m01 * m.m22) * invdet;
		result.m02 = (m.m01 * m.m12 - m.m02 * m.m11) * invdet;
		result.m10 = (m.m12 * m.m20 - m.m10 * m.m22) * invdet;
		result.m11 = (m.m00 * m.m22 - m.m02 * m.m20) * invdet;
		result.m12 = (m.m10 * m.m02 - m.m00 * m.m12) * invdet;
		result.m20 = (m.m10 * m.m21 - m.m20 * m.m11) * invdet;
		result.m21 = (m.m20 * m.m01 - m.m00 * m.m21) * invdet;
		result.m22 = (m.m00 * m.m11 - m.m10 * m.m01) * invdet;
	}

	private void InverseMatrix2x2(ref Matrix4x4 m, ref Matrix4x4 result)
	{         
		float invdet = 1.0f / (m.m00*m.m11 - m.m01*m.m10);
		result.m00 =  m.m11 * invdet;
		result.m01 = -m.m01 * invdet;
		result.m10 = -m.m10 * invdet;
		result.m11 =  m.m00 * invdet;
	}
}