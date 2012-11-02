using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RUISPointTracker : MonoBehaviour {
    public class PointData
    {
        public Vector3 position;
        public Quaternion rotation;
        public float deltaTime;
        public Vector3 velocity;

        public PointData(Vector3 position, Quaternion rotation, float deltaTime, PointData previous){
            this.position = position;
            this.rotation = rotation;
            this.deltaTime = deltaTime;
            if (previous != null)
            {
                velocity = (position - previous.position) / deltaTime;
            }
        }
    }

    Queue<PointData> points = new Queue<PointData>();
    PointData previousPoint = null;

    public float updateInterval;
    public int bufferSize;

    float timeSinceLastUpdate = 0;

    void Awake()
    {
        cachedAverageSpeed = new CachedAverageSpeed(points);
        cachedMaxVelocity = new CachedMaxVelocity(points);
    }

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate < updateInterval) return;

        PointData newPoint = new PointData(transform.position, transform.rotation, timeSinceLastUpdate, previousPoint);
        points.Enqueue(newPoint);
        previousPoint = newPoint;

        if (points.Count > bufferSize) points.Dequeue();

        InvalidateCaches();

        //Debug.Log(averageSpeed);

        timeSinceLastUpdate = 0;
    }

    private void InvalidateCaches()
    {
        cachedAverageSpeed.Invalidate();
        cachedMaxVelocity.Invalidate();
    }

    private CachedAverageSpeed cachedAverageSpeed;
    public float averageSpeed
    {
        get
        {
            return cachedAverageSpeed.GetValue();
        }
    }

    private CachedMaxVelocity cachedMaxVelocity;
    public Vector3 maxVelocity
    {
        get
        {
            return cachedMaxVelocity.GetValue();
        }
    }



    public class CachedAverageSpeed : CachedValue<float>
    {
        Queue<PointData> valueList;

        public CachedAverageSpeed(Queue<PointData> valueList)
        {
            this.valueList = valueList;
        }

        protected override float CalculateValue()
        {
            float speed = 0;
            foreach (PointData data in valueList)
            {
                speed += data.velocity.magnitude;
            }
            return speed / valueList.Count;
        }
    }

    public class CachedMaxVelocity : CachedValue<Vector3>
    {
        Queue<PointData> valueList;

        public CachedMaxVelocity(Queue<PointData> valueList)
        {
            this.valueList = valueList;
        }

        protected override Vector3 CalculateValue()
        {
            Vector3 maxVelocity = Vector3.zero;
            foreach (PointData data in valueList)
            {
                maxVelocity = maxVelocity.sqrMagnitude > data.velocity.sqrMagnitude ? maxVelocity : data.velocity;
            }

            return maxVelocity;
        }
    }

    
}
