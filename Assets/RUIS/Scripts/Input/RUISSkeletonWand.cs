using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("RUIS/Input/RUISSkeletonWand")]
public class RUISSkeletonWand : RUISWand
{
    public RUISSkeletonManager.Joint wandStart = RUISSkeletonManager.Joint.RightElbow;
    public RUISSkeletonManager.Joint wandEnd = RUISSkeletonManager.Joint.RightHand;

    public RUISGestureRecognizer gestureRecognizer;

    public RUISSkeletonManager skeletonManager;

    RUISDisplayManager displayManager;

    public Color wandColor = Color.white;

    public int playerId = 0;

    private const int amountOfSelectionVisualizerImages = 8;
    Texture2D[] selectionVisualizers;

    public int visualizerWidth = 64;
    public int visualizerHeight = 64;

    public float visualizerThreshold = 0.25f;

    public void Awake()
    {
        if (!skeletonManager)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }

        selectionVisualizers = new Texture2D[8];
        for (int i = 0; i < amountOfSelectionVisualizerImages; i++)
        {
            selectionVisualizers[i] = Resources.Load("RUIS/Graphics/Selection/visualizer" + (i + 1)) as Texture2D;
        }

        displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;

        if (!gestureRecognizer)
        {
            Debug.LogWarning("Please set a gesture recognizer for wand: " + name + " if you want to use gestures.");
        }
    }

    public void Update()
    {
        if (!skeletonManager.skeletons[playerId].isTracking) return;

        visualizerThreshold = Mathf.Clamp01(visualizerThreshold);

        RUISSkeletonManager.JointData startData = skeletonManager.GetJointData(wandStart, playerId);
        RUISSkeletonManager.JointData endData = skeletonManager.GetJointData(wandEnd, playerId);

        if (endData.positionConfidence >= 0.5f)
        {
            transform.position = endData.position;

            if (startData != null && startData.positionConfidence >= 0.5f)
            {
                transform.rotation = Quaternion.LookRotation(endData.position - startData.position);
            }
            else if (endData.rotationConfidence >= 0.5f)
            {
                transform.rotation = endData.rotation;
            }
        }

        if (gestureRecognizer)
            gestureRecognizer.GestureTriggered();
    }

    public void OnGUI()
    {
        if (!skeletonManager.skeletons[playerId].isTracking) return;

        float gestureProgress = gestureRecognizer.GetGestureProgress();

        if (gestureProgress >= visualizerThreshold)
        {
            float visualizerPhase = (gestureProgress - visualizerThreshold) / (1 - visualizerThreshold);
            int selectionVisualizerIndex = (int)(amountOfSelectionVisualizerImages * visualizerPhase);
            selectionVisualizerIndex = Mathf.Clamp(selectionVisualizerIndex, 0, amountOfSelectionVisualizerImages - 1);

            List<RUISDisplayManager.ScreenPoint> screenPoints = displayManager.WorldPointToScreenPoints(transform.position);

            foreach (RUISDisplayManager.ScreenPoint screenPoint in screenPoints)
            {
                RUISGUI.DrawTextureViewportSafe(new Rect(screenPoint.coordinates.x - visualizerWidth / 2, screenPoint.coordinates.y - visualizerHeight / 2, visualizerWidth, visualizerHeight),
                    screenPoint.camera, selectionVisualizers[selectionVisualizerIndex]);
                //GUI.DrawTexture(new Rect(screenPoint.x - visualizerWidth / 2, screenPoint.y - visualizerHeight / 2, visualizerWidth, visualizerHeight), selectionVisualizers[selectionVisualizerIndex]);
            }
        }

    }

    public override bool SelectionButtonWasPressed()
    {
        if (!skeletonManager.skeletons[playerId].isTracking || !gestureRecognizer) return false;
        return gestureRecognizer.GestureTriggered();
    }

    public override bool SelectionButtonWasReleased()
    {
        if (!skeletonManager.skeletons[playerId].isTracking || !gestureRecognizer) return false;
        return gestureRecognizer.GestureTriggered();
    }

    public override bool SelectionButtonIsDown()
    {
        if (!skeletonManager.skeletons[playerId].isTracking || !gestureRecognizer) return false;
        return gestureRecognizer.GestureTriggered();
    }

    public override Vector3 GetAngularVelocity()
    {
        return Vector3.zero;
    }

    public override Color color { get { return wandColor; } }
}
