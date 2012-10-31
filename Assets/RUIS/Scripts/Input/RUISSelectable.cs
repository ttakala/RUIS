using UnityEngine;
using System.Collections.Generic;

public class RUISSelectable : MonoBehaviour {

    private bool rigidbodyWasKinematic;
    private RUISWandSelector selector;
    public bool isSelected { get { return selector != null; } }

    private Vector3 positionAtSelection;
    private Quaternion rotationAtSelection;
    private Vector3 selectorPositionAtSelection;
    private Quaternion selectorRotationAtSelection;
    private float distanceFromSelectionRayOrigin;
    public float DistanceFromSelectionRayOrigin
    {
        get
        {
            return distanceFromSelectionRayOrigin;
        }
    }

    public bool clampToCertainDistance = false;
    public float distanceToClampTo = 1.0f;
    
    //for highlights
    public Material highlightMaterial;
    public Material selectionMaterial;

    public bool maintainMomentumAfterRelease = true;

    private Vector3 latestVelocity = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;

    private Queue<Vector3> velocityBuffer;

    public void Awake()
    {
        velocityBuffer = new Queue<Vector3>();
    }

    public void Start()
    {
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        UpdateTransform(true);

        /*if (rigidbody)
        {
            latestVelocity = (rigidbody.velocity != Vector3.zero) ? rigidbody.velocity : latestVelocity;
        }*/

        latestVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;

        velocityBuffer.Enqueue(latestVelocity);
        LimitBufferSize(velocityBuffer, 5);
    }

    public virtual void OnSelection(RUISWandSelector selector)
    {
        this.selector = selector;

        positionAtSelection = transform.position;
        rotationAtSelection = transform.rotation;
        selectorPositionAtSelection = selector.transform.position;
        selectorRotationAtSelection = selector.transform.rotation;
        distanceFromSelectionRayOrigin = (positionAtSelection - selector.selectionRay.origin).magnitude;

        lastPosition = transform.position;

        if (rigidbody)
        {
            rigidbodyWasKinematic = rigidbody.isKinematic;
            rigidbody.isKinematic = true;
        }

        AddMaterialToEverything(selectionMaterial);

        UpdateTransform(false);
    }


    public virtual void OnSelectionEnd()
    {
        if (rigidbody)
        {
            rigidbody.isKinematic = rigidbodyWasKinematic;
        }

        if (maintainMomentumAfterRelease && rigidbody && !rigidbody.isKinematic)
        {
            rigidbody.AddForce(AverageBufferContent(velocityBuffer), ForceMode.VelocityChange);

            rigidbody.AddTorque(Mathf.Deg2Rad * selector.angularVelocity, ForceMode.VelocityChange);
        }

        RemoveMaterialFromEverything();

        this.selector = null;
    }

    public virtual void OnSelectionHighlight()
    {
        AddMaterialToEverything(highlightMaterial);
    }

    public virtual void OnSelectionHighlightEnd()
    {
        RemoveMaterialFromEverything();
    }

    protected virtual void UpdateTransform(bool safePhysics)
    {
        if (!isSelected) return;

        Vector3 newPosition = transform.position;
        Quaternion newRotation = transform.rotation;

        switch (selector.positionSelectionGrabType)
        {
            case RUISWandSelector.SelectionGrabType.SnapToWand:
                newPosition = selector.transform.position;
                break;
            case RUISWandSelector.SelectionGrabType.RelativeToWand:
                Vector3 selectorPositionChange = selector.transform.position - selectorPositionAtSelection;
                newPosition = positionAtSelection + selectorPositionChange;
                break;
            case RUISWandSelector.SelectionGrabType.AlongSelectionRay:
                float clampDistance = distanceFromSelectionRayOrigin;
                if (clampToCertainDistance) clampDistance = distanceToClampTo;
                newPosition = selector.selectionRay.origin + clampDistance * selector.selectionRay.direction;
                break;
        }

        switch (selector.rotationSelectionGrabType)
        {
            case RUISWandSelector.SelectionGrabType.SnapToWand:
                newRotation = selector.transform.rotation;
                break;
            case RUISWandSelector.SelectionGrabType.RelativeToWand:
                newRotation = rotationAtSelection;
                Quaternion selectorRotationChange = (Quaternion.Inverse(selectorRotationAtSelection) * selector.transform.rotation);
                //transform.Rotate(selectorRotationChange, Space.World);
                newRotation = selectorRotationChange * newRotation;//newRotation * selectorRotationChange;
                break;
            case RUISWandSelector.SelectionGrabType.AlongSelectionRay:
                newRotation = Quaternion.LookRotation(selector.selectionRay.direction);
                break;
        }

        if (rigidbody && safePhysics)
        {
            rigidbody.MovePosition(newPosition);
            rigidbody.MoveRotation(newRotation);
        }
        else
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
        }
    }

    private void LimitBufferSize(Queue<Vector3> buffer, int maxSize)
    {
        while (buffer.Count >= maxSize)
        {
            buffer.Dequeue();
        }
    }

    private Vector3 AverageBufferContent(Queue<Vector3> buffer)
    {
        int startingBufferSize = buffer.Count;
        Vector3 averagedContent = new Vector3();
        while (buffer.Count > 0)
        {
            averagedContent += buffer.Dequeue() / startingBufferSize;
        }

        return averagedContent;
    }

    private void AddMaterial(Material m, Renderer r)
    {
        if (m == null || r == null) return;

        Material[] newMaterials = new Material[r.materials.Length + 1];
        for (int i = 0; i < r.materials.Length; i++)
        {
            newMaterials[i] = r.materials[i];
        }

        newMaterials[newMaterials.Length - 1] = m;
        r.materials = newMaterials;
    }

    private void RemoveMaterial(Renderer r)
    {
        if (r == null) return;

        Material[] newMaterials = new Material[r.materials.Length - 1];
        for (int i = 0; i < newMaterials.Length; i++)
        {
            newMaterials[i] = r.materials[i];
        }
        r.materials = newMaterials;
    }

    private void AddMaterialToEverything(Material m)
    {
        AddMaterial(m, renderer);

        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            AddMaterial(m, childRenderer);
        }
    }

    private void RemoveMaterialFromEverything()
    {
        RemoveMaterial(renderer);

        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            RemoveMaterial(childRenderer);
        }
    }
}
