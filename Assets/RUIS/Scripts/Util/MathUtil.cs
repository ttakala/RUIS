/*****************************************************************************

Content    :   Basic math utility functions
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using CSML;

public class MathUtil {
    //Uses the stabilized Gram-Schmidt process to orthonormalize a set of vectors
    public static List<Vector3> Orthonormalize(List<Vector3> vectors)
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < vectors.Count; i++)
        {
            result.Add(vectors[i].normalized);

            for (int j = i + 1; j < vectors.Count; j++)
            {
                vectors[j] -= Vector3.Project(vectors[j], result[i]);
            }
        }

        return result;
    }

    //Copies a CSML transformation matrix into a Matrix4x4 transformation. Only uses a 3x4 part of the CSML matrix, since it's supposed to be a transformation.
    public static Matrix4x4 MatrixToMatrix4x4(Matrix mat)
    {
        Matrix4x4 result = new Matrix4x4();

        for (int i = 0; i < 4 && i < mat.RowCount; i++)
        {
            for (int j = 0; j < 4 && j < mat.ColumnCount; j++)
            {
                result[i, j] = (float)mat[i + 1, j + 1].Re;
            }
        }

        result[3, 3] = 1.0f;

        return result;
    }

    //Extracts the rotation columns out of a Matrix4x4
    public static List<Vector3> ExtractRotationVectors(Matrix4x4 m)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < 3; i++)
        {
            Vector3 column = new Vector3(m[0, i], m[1, i], m[2, i]);
            result.Add(column);
        }

        return result;
    }

    //Converts a Matrix4x4 rotation matrix to a Quaternion
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }

    public static float CalculateStandardDeviation(IList<float> values)
    {
        float variance = 0;
        foreach (float value in values)
        {
            variance += Mathf.Pow(value, 2);
        }

        variance /= values.Count;

        return Mathf.Sqrt(variance);
    }
}
