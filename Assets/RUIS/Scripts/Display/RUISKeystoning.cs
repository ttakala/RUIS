using UnityEngine;
using System.Collections;

public class RUISKeystoning {
    private const float maxCornerSelectionDistance = 0.05f;

    public class KeystoningSpecification
    {
        public float a, b, d, e, f, h, i, j;

        public KeystoningSpecification()
        {
            b = d = e = h = i = j = 0;
            a = f = 1;
        }

        private KeystoningSpecification(KeystoningSpecification other)
        {
            this.a = other.a;
            this.b = other.b;
            this.d = other.d;
            this.e = other.e;
            this.f = other.f;
            this.h = other.h;
            this.i = other.i;
            this.j = other.j;
        }

        public Matrix4x4 GetMatrix(){
            Matrix4x4 keystone = new Matrix4x4();
            // a b 0 d
            // e f 0 h
            // m n 1 0
            // 0 0 0 1
            keystone[0, 0] = a;
            keystone[0, 1] = b;
            keystone[0, 3] = d;
            keystone[1, 0] = e;
            keystone[1, 1] = f;
            keystone[1, 3] = h;
            keystone[2, 0] = i;
            keystone[2, 1] = j;
            keystone[2, 2] = 1;
            keystone[3, 3] = 1;

            return keystone;
        }

        public KeystoningSpecification ModifyWithStep(int whichValue, float modification)
        {
            KeystoningSpecification newSpec = new KeystoningSpecification(this);

            switch (whichValue)
            {
                case 0:
                    newSpec.a += modification;
                    break;
                case 1:
                    newSpec.b += modification;
                    break;
                case 2:
                    newSpec.d += modification;
                    break;
                case 3:
                    newSpec.e += modification;
                    break;
                case 4:
                    newSpec.f += modification;
                    break;
                case 5:
                    newSpec.h += modification;
                    break;
                case 6:
                    newSpec.i += modification;
                    break;
                case 7:
                    newSpec.j += modification;
                    break;
            }

            return newSpec;
        }
    }

    public class KeystoningCorners
    {
        Vector2[] corners;

        public KeystoningCorners()
        {
            corners = new Vector2[4];
            corners[0] = new Vector2(0, 1);
            corners[1] = new Vector2(1, 1);
            corners[2] = new Vector2(1, 0);
            corners[3] = new Vector2(0, 0);
        }

        public Vector2 this[int i]
        {
            get { return corners[i]; }
            set { corners[i] = value; }
        }

        public Vector2 GetDiagonalCenter()
        {
            if (corners[2].x - corners[0].x == 0)
            {
                return corners[0];
            }

            if (corners[1].x - corners[3].x == 0)
            {
                return corners[3];
            }

            float m1 = (corners[2].y - corners[0].y) / (corners[2].x - corners[0].x);
            float m2 = (corners[1].y - corners[3].y) / (corners[1].x - corners[3].x);

            float x = (m2 * corners[1].x - m1 * corners[0].x + corners[0].y - corners[1].y) / (m2 - m1);
            return new Vector2(x, m1 * x + corners[0].y - m1 * corners[0].x);
        }

        public int GetClosestCornerIndex(Vector2 toPosition)
        {
            float closestDist = 100;
            int closestCorner = -1;
            for (int i = 0; i < 4; i++)
            {
                float newDist = Vector2.Distance(toPosition, corners[i]);
                if (newDist < closestDist)
                {
                    closestDist = newDist;
                    closestCorner = i;
                }
            }

            if (closestDist > maxCornerSelectionDistance)
            {
                return -1;
            }

            return closestCorner;
        }
    }



    public static KeystoningSpecification Optimize(Camera camera, KeystoningCorners corners)
    {
        KeystoningSpecification spec = new KeystoningSpecification();
        spec.a = 1;
        spec.f = 1;

        //save all the relevant world points that we use for optimizing the keystoning matrix
        camera.ResetProjectionMatrix();
        Matrix4x4 originalProjectionMatrix = camera.projectionMatrix;
        Vector3[] worldPoints = new Vector3[10];
        worldPoints[0] = camera.ViewportToWorldPoint(new Vector3(0, 1, camera.nearClipPlane));
        worldPoints[1] = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.nearClipPlane));
        worldPoints[2] = camera.ViewportToWorldPoint(new Vector3(1, 0, camera.nearClipPlane));
        worldPoints[3] = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        worldPoints[4] = camera.ViewportToWorldPoint(new Vector3(0, 1, camera.farClipPlane));
        worldPoints[5] = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.farClipPlane));
        worldPoints[6] = camera.ViewportToWorldPoint(new Vector3(1, 0, camera.farClipPlane));
        worldPoints[7] = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.farClipPlane));
        worldPoints[8] = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane));
        worldPoints[9] = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.farClipPlane));

        /*Debug.Log("---------------------------------------------------------------");
        foreach (Vector3 worldPoint in worldPoints)
        {
            Debug.Log(worldPoint);
            Debug.Log(camera.WorldToViewportPoint(worldPoint));
        }
        Debug.Log("--------------------------------------------------------------");*/

        float stepSize = 0.1f;
        float[] grad = new float[8];

        while (stepSize > 0.000001f)
        {
            // compute gradient

            float currentValue = CalcValue(spec, camera, originalProjectionMatrix, corners, worldPoints);
            /*;

            Debug.Log("**********************************************");
            foreach (Vector3 worldPoint in worldPoints)
            {
                Debug.Log(camera.WorldToViewportPoint(worldPoint));
            }
            Debug.Log("**********************************************");
            */
            //Debug.Log(currentValue);
            grad[0] = CalcValue(spec.ModifyWithStep(0, 0.1f * stepSize), camera, originalProjectionMatrix, corners, worldPoints) - currentValue;
            grad[1] = CalcValue(spec.ModifyWithStep(1, 0.1f * stepSize), camera, originalProjectionMatrix, corners, worldPoints) - currentValue;
            grad[2] = CalcValue(spec.ModifyWithStep(2, 0.1f * stepSize), camera, originalProjectionMatrix, corners, worldPoints) - currentValue;
            grad[3] = CalcValue(spec.ModifyWithStep(3, 0.1f * stepSize), camera, originalProjectionMatrix, corners, worldPoints) - currentValue;
            grad[4] = CalcValue(spec.ModifyWithStep(4, 0.1f * stepSize), camera, originalProjectionMatrix, corners, worldPoints) - currentValue;
            grad[5] = CalcValue(spec.ModifyWithStep(5, 0.1f * stepSize), camera, originalProjectionMatrix, corners, worldPoints) - currentValue;
            grad[6] = CalcValue(spec.ModifyWithStep(6, 0.1f * stepSize), camera, originalProjectionMatrix, corners, worldPoints) - currentValue;
            grad[7] = CalcValue(spec.ModifyWithStep(7, 0.1f * stepSize), camera, originalProjectionMatrix, corners, worldPoints) - currentValue;
            // step in direction of gradient with length of stepSize
            float gradSquaredLength = 0;
            for (int i = 0; i < 8; i++)
            {
                gradSquaredLength += grad[i] * grad[i];
            }
            float gradLength = Mathf.Sqrt((float)gradSquaredLength);
            for (int i = 0; i < 8; i++)
            {
                spec = spec.ModifyWithStep(i, -stepSize * grad[i] / gradLength);
            }
            stepSize *= 0.99f;
        }

        return spec;
    }

    //compares projected world points to the expected corner points and calculates the total square distance
    private static float CalcValue(KeystoningSpecification spec, Camera camera, Matrix4x4 originalProjectionMatrix, KeystoningCorners corners, Vector3[] worldPoints)
    {
        //Debug.Log(CreateKeystonedMatrix(a, b, d, e, f, h, m, n));
        camera.projectionMatrix = originalProjectionMatrix * spec.GetMatrix();


        
        Vector2[] newPoints = new Vector2[worldPoints.Length];
        for(int i = 0; i < worldPoints.Length; i++)
        {
            newPoints[i] = camera.WorldToViewportPoint(worldPoints[i]);
        }

        float distanceFromCorners = 0; // sum of squared distances
        for (int i = 0; i < 4; i++)
        {
            distanceFromCorners += (corners[i] - newPoints[i]).sqrMagnitude;
            distanceFromCorners += (corners[i] - newPoints[i+4]).sqrMagnitude;
        }

        Vector2 diagonalCenter = corners.GetDiagonalCenter();
        distanceFromCorners += (diagonalCenter - newPoints[8]).sqrMagnitude;
        distanceFromCorners += (diagonalCenter - newPoints[9]).sqrMagnitude;

        return distanceFromCorners;
    }
}
