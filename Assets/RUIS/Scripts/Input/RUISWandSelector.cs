using UnityEngine;
using System.Collections;

[System.Serializable]
public class RUISWandSelector : MonoBehaviour {
    public enum SelectionRayType {
        HeadToWand,
        WandDirection
    };

    public SelectionRayType selectionRayType = SelectionRayType.WandDirection;
    public LineRenderer lineRenderer;
    public float selectionRayLength = 200;
    public float selectionRayStartDistance = 0.2f;
    private Vector3 selectionRayStart;
    private Vector3 selectionRayEnd;
    public Ray selectionRay { get; private set; }

    public Transform headTransform;

    public bool toggleSelection;

    public enum SelectionGrabType
    {
        SnapToWand,
        RelativeToWand,
        AlongSelectionRay
    }

    public SelectionGrabType positionSelectionGrabType = SelectionGrabType.SnapToWand;
    public SelectionGrabType rotationSelectionGrabType = SelectionGrabType.SnapToWand;
    
    private RUISWand wand;
    private RUISSelectable selection;
    public RUISSelectable Selection
    {
        get
        {
            return selection;
        }
    }

    private RUISSelectable highlightedObject;

    public Vector3 angularVelocity
    {
        get
        {
            return wand.GetAngularVelocity();
        }
    }

    public void Awake()
    {
        wand = GetComponent<RUISWand>();

        if (!wand)
        {
            Debug.LogError(name + ": RUISWandSelector requires a RUISWand");
        }

        if (lineRenderer)
        {
            lineRenderer.SetVertexCount(2);
        }
    }

    public void Update()
    {
        GameObject selectionGameObject = CheckForSelection();
        if (!selection && selectionGameObject)
        {
            RUISSelectable selectableObject = selectionGameObject.GetComponent<RUISSelectable>();
            if (selectableObject && !selectableObject.isSelected)
            {
                if (selectableObject != highlightedObject)
                {
                    if (highlightedObject != null) highlightedObject.OnSelectionHighlightEnd();

                    selectableObject.OnSelectionHighlight();
                    highlightedObject = selectableObject;
                }

                if (wand.SelectionButtonWasPressed())
                {
                    selection = selectableObject;

                    if (highlightedObject != null)
                    {
                        highlightedObject.OnSelectionHighlightEnd();
                        highlightedObject = null;
                    }

                    BeginSelection();
                }
            }
        }
        else if (selection && ((!toggleSelection && wand.SelectionButtonWasReleased()) || (toggleSelection && wand.SelectionButtonWasPressed())))
        {
            EndSelection();
        }

        if (!selectionGameObject || !selectionGameObject.GetComponent<RUISSelectable>())
        {
            if (highlightedObject != null)
            {
                highlightedObject.OnSelectionHighlightEnd();
                highlightedObject = null;
            }
        }
    }

    void LateUpdate()
    {
        UpdateLineRenderer();
    }

    private GameObject CheckForSelection()
    {
        switch (selectionRayType)
        {
            case SelectionRayType.HeadToWand:
                RaycastHit headToWandHit;
                selectionRay = new Ray(headTransform.position, transform.position - headTransform.position);

                if (Physics.Raycast(selectionRay, out headToWandHit, selectionRayLength))
                {
                    selectionRayEnd = headToWandHit.point;
                    return headToWandHit.collider.gameObject;
                }
                break;
            case SelectionRayType.WandDirection:
                RaycastHit wandDirectionHit;
                selectionRay = new Ray(transform.position, transform.forward);

                if (Physics.Raycast(selectionRay, out wandDirectionHit, selectionRayLength))
                {
                    selectionRayEnd = wandDirectionHit.point;
                    return wandDirectionHit.collider.gameObject;
                }
                else
                {
                    selectionRayEnd = transform.position + selectionRayLength * transform.forward;
                }
                break;
        }
        return null;
    }

    private void BeginSelection()
    {
        selection.OnSelection(this);
    }

    private void EndSelection()
    {
        selection.OnSelectionEnd();
        selection = null;
    }

    private void UpdateLineRenderer()
    {
        if(!lineRenderer) return;

        lineRenderer.enabled = selection == null && selectionRayType == SelectionRayType.WandDirection;

        lineRenderer.SetColors(wand.color, wand.color);

        lineRenderer.SetPosition(0, selectionRay.origin + selectionRayStartDistance * selectionRay.direction);
        lineRenderer.SetPosition(1, selectionRayEnd);
    }
}
