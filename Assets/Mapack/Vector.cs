using UnityEngine;
using System.Collections;

public class Vector {
	private double[] array;

	/// <summary>Constructs a vector of the given length and assigns a given value to all elements.</summary>
	/// <param name="length">Vector length.</param>
	/// <param name="value">Value to assign to the elements.</param>
	public Vector(int length, double value)
	{
		this.array = new double[length];
		
		for (int i = 0; i < length; i++)
		{
			array[i] = value;
		}
	}
	
	public int Length
	{
		get 
		{ 
			return array.Length; 
		}
	}
}
