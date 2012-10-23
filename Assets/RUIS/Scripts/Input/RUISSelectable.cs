using UnityEngine;
using System.Collections.Generic;

public class RUISSelectable : MonoBehaviour {
    public enum SelectionGrabType
    {
        SnapToWand,
        RelativeToWand,
        AlongSelectionRay
    }

    public SelectionGrabType positionSelectionGrabType = SelectionGrabType.SnapToWand;
    public SelectionGrabType rotationSelectionGrabType = SelectionGrabType.RelativeToWand;

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
    private bool isHighlighted = false;
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

        AddMaterial(selectionMaterial);

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

        RemoveMaterial();

        this.selector = null;
    }

    public virtual void OnSelectionHighlight()
    {
        isHighlighted = true;
        AddMaterial(highlightMaterial);
    }

    public virtual void OnSelectionHighlightEnd()
    {
        RemoveMaterial();
    }

    protected virtual void UpdateTransform(bool safePhysics)
    {
        if (!isSelected) return;

        Vector3 newPosition = transform.position;
        Quaternion newRotation = transform.rotation;

        switch (positionSelectionGrabType)
        {
            case SelectionGrabType.SnapToWand:
                newPosition = selector.transform.position;
                break;
            case SelectionGrabType.RelativeToWand:
                Vector3 selectorPositionChange = selector.transform.position - selectorPositionAtSelection;
                newPosition = positionAtSelection + selectorPositionChange;
                break;
            case SelectionGrabType.AlongSelectionRay:
                float clampDistance = distanceFromSelectionRayOrigin;
                if (clampToCertainDistance) clampDistance = distanceToClampTo;
                newPosition = selector.selectionRay.origin + clampDistance * selector.selectionRay.direction;
                break;
        }

        switch (rotationSelectionGrabType)
        {
            case SelectionGrabType.SnapToWand:
                newRotation = selector.transform.rotation;
                break;
            case SelectionGrabType.RelativeToWand:
                newRotation = rotationAtSelection;
                Vector3 selectorRotationChange = (Quaternion.Inverse(selectorRotationAtSelection) * selector.transform.rotation).eulerAngles;
                //transform.Rotate(selectorRotationChange, Space.World);
                newRotation = newRotation * Quaternion.Euler(selectorRotationChange);
                break;
            case SelectionGrabType.AlongSelectionRay:
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

    private void AddMaterial(Material material)
    {
        Material[] newMaterials = new Material[renderer.materials.Length + 1];
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            newMaterials[i] = renderer.materials[i];
        }

        newMaterials[newMaterials.Length - 1] = material;
        renderer.materials = newMaterials;
    }

    private void RemoveMaterial()
    {
        Material[] newMaterials = new Material[renderer.materials.Length - 1];
        for (int i = 0; i < newMaterials.Length; i++)
        {
            newMaterials[i] = renderer.materials[i];
        }
        renderer.materials = newMaterials;
    }
}
