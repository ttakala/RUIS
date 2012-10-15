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

    public bool clampToCertainDistance = false;
    public float distanceToClampTo = 1.0f;
    
    //for highlights
    private bool isHighlighted = false;
    public Material highlightMaterial;
    public Material selectionMaterial;

    public bool maintainMomentumAfterRelease = true;
    private Queue<Vector3> velocityBuffer;
    public int velocityBufferSize = 5;
    private Queue<Vector3> rotationalVelocityBuffer;
    public int rotationalVelocityBufferSize = 5;

    public void Awake()
    {
        velocityBuffer = new Queue<Vector3>();
        rotationalVelocityBuffer = new Queue<Vector3>();
    }

    public void Start()
    {
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        Vector3 oldPosition = transform.position;
        Vector3 oldRotation = transform.rotation.eulerAngles;

        UpdateTransform();

        Vector3 velocity = (transform.position - oldPosition) / Time.fixedDeltaTime;
        velocityBuffer.Enqueue(velocity);
        LimitBufferSize(velocityBuffer, velocityBufferSize);

        Vector3 rotationalVelocity = (transform.rotation.eulerAngles - oldRotation) / Time.fixedDeltaTime;
        rotationalVelocityBuffer.Enqueue(rotationalVelocity);
        LimitBufferSize(rotationalVelocityBuffer, rotationalVelocityBufferSize);
        
    }

    public virtual void OnSelection(RUISWandSelector selector)
    {
        this.selector = selector;

        positionAtSelection = transform.position;
        rotationAtSelection = transform.rotation;
        selectorPositionAtSelection = selector.transform.position;
        selectorRotationAtSelection = selector.transform.rotation;
        distanceFromSelectionRayOrigin = (positionAtSelection - selector.selectionRay.origin).magnitude;

        if (rigidbody)
        {
            rigidbodyWasKinematic = rigidbody.isKinematic;
            rigidbody.isKinematic = true;
        }

        velocityBuffer.Clear();

        AddMaterial(selectionMaterial);
    }


    public virtual void OnSelectionEnd()
    {
        Vector3 oldVelocity = Vector3.zero;

        if (rigidbody)
        {
            oldVelocity = rigidbody.velocity;
            rigidbody.isKinematic = rigidbodyWasKinematic;
        }


        Debug.Log(oldVelocity);

        if (maintainMomentumAfterRelease && rigidbody && !rigidbody.isKinematic)
        {
            //give the rigidbody speed based on its current velocity
            Vector3 velocity = selector.velocity;
            
            if (positionSelectionGrabType == SelectionGrabType.AlongSelectionRay)
            {
                //velocity = AverageBufferContent(velocityBuffer);
                velocity = oldVelocity;
            }
            rigidbody.AddForce(velocity, ForceMode.VelocityChange);

            Vector3 bufferedRotationalVelocity = AverageBufferContent(rotationalVelocityBuffer);
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

    protected virtual void UpdateTransform()
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

        if (rigidbody)
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
