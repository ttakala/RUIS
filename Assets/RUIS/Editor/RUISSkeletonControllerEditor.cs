/*****************************************************************************

Content    :   Inspector behaviour for RUISKinectAndMecanimCombiner script
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISSkeletonController))]
[CanEditMultipleObjects]
public class RUISSkeletonControllerEditor : Editor
{
    SerializedProperty playerId;

    SerializedProperty useHierarchicalModel;

    SerializedProperty updateRootPosition;
    SerializedProperty updateJointPositions;
    SerializedProperty updateJointRotations;

    SerializedProperty scaleHierarchicalModelBones;

    SerializedProperty rootBone;
    SerializedProperty headBone;
    SerializedProperty neckBone;
    SerializedProperty torsoBone;

    SerializedProperty leftShoulderBone;
    SerializedProperty leftElbowBone;
    SerializedProperty leftHandBone;
    SerializedProperty rightShoulderBone;
    SerializedProperty rightElbowBone;
    SerializedProperty rightHandBone;

    SerializedProperty leftHipBone;
    SerializedProperty leftKneeBone;
    SerializedProperty leftFootBone;
    SerializedProperty rightHipBone;
    SerializedProperty rightKneeBone;
    SerializedProperty rightFootBone;

    SerializedProperty maxScaleFactor;
    SerializedProperty minimumConfidenceToUpdate;
    SerializedProperty rotationDamping;
    SerializedProperty neckHeightTweaker;
    SerializedProperty forearmLengthTweaker;

    public void OnEnable()
    {
        playerId = serializedObject.FindProperty("playerId");

        useHierarchicalModel = serializedObject.FindProperty("useHierarchicalModel");

        updateRootPosition = serializedObject.FindProperty("updateRootPosition");
        updateJointPositions = serializedObject.FindProperty("updateJointPositions");
        updateJointRotations = serializedObject.FindProperty("updateJointRotations");

        scaleHierarchicalModelBones = serializedObject.FindProperty("scaleHierarchicalModelBones");

        rootBone = serializedObject.FindProperty("root");
        headBone = serializedObject.FindProperty("head");
        neckBone = serializedObject.FindProperty("neck");
        torsoBone = serializedObject.FindProperty("torso");

        leftShoulderBone = serializedObject.FindProperty("leftShoulder");
        leftElbowBone = serializedObject.FindProperty("leftElbow");
        leftHandBone = serializedObject.FindProperty("leftHand");
        rightShoulderBone = serializedObject.FindProperty("rightShoulder");
        rightElbowBone = serializedObject.FindProperty("rightElbow");
        rightHandBone = serializedObject.FindProperty("rightHand");

        leftHipBone = serializedObject.FindProperty("leftHip");
        leftKneeBone = serializedObject.FindProperty("leftKnee");
        leftFootBone = serializedObject.FindProperty("leftFoot");
        rightHipBone = serializedObject.FindProperty("rightHip");
        rightKneeBone = serializedObject.FindProperty("rightKnee");
        rightFootBone = serializedObject.FindProperty("rightFoot");

        maxScaleFactor = serializedObject.FindProperty("maxScaleFactor");
        minimumConfidenceToUpdate = serializedObject.FindProperty("minimumConfidenceToUpdate");
        rotationDamping = serializedObject.FindProperty("rotationDamping");
        neckHeightTweaker = serializedObject.FindProperty("neckHeightTweaker");
        forearmLengthTweaker = serializedObject.FindProperty("forearmLengthRatio");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(playerId, new GUIContent("Player Id", "The kinect player id number"));
        
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(useHierarchicalModel, new GUIContent("Use Hierarchical Model", "Is the model armature hierarchical (a tree) instead of non-hierarchical (all bones are on same level)"));

        EditorGUILayout.PropertyField(updateRootPosition, new GUIContent("Update Root Position", "Update the position of this GameObject according to the skeleton root position"));

        GUI.enabled = !useHierarchicalModel.boolValue;
        EditorGUILayout.PropertyField(updateJointPositions, new GUIContent("Update Joint Positions", "Update all joint positions, this shouldn't be done on hierarchical armatures, since the skeleton structure already handles positions with joint rotations."));
        if (useHierarchicalModel.boolValue) updateJointPositions.boolValue = false;
        GUI.enabled = true;

        EditorGUILayout.PropertyField(updateJointRotations, new GUIContent("Update Joint Rotations", "Update all joint rotations, especially important for hierarchical models"));

        GUI.enabled = useHierarchicalModel.boolValue;
        EditorGUILayout.PropertyField(scaleHierarchicalModelBones, new GUIContent("Scale Bones", "Scale the bones of the model based on the real-life lengths of the player bones"));
        if (!useHierarchicalModel.boolValue) scaleHierarchicalModelBones.boolValue = false;
        GUI.enabled = true;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Torso and Head", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rootBone, new GUIContent("Root Bone", "The skeleton hierarchy root bone"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(headBone, new GUIContent("Head", "The head bone"));
        EditorGUILayout.PropertyField(neckBone, new GUIContent("Neck", "The neck bone"));
        EditorGUILayout.PropertyField(torsoBone, new GUIContent("Torso", "The torso bone, has to be parent or grandparent of the hips"));

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Arms", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
            EditorGUILayout.PropertyField(leftShoulderBone, new GUIContent("Left Shoulder", "The left shoulder bone (upper arm)"));
            EditorGUILayout.PropertyField(leftElbowBone, new GUIContent("Left Elbow", "The left elbow bone (forearm)"));
            EditorGUILayout.PropertyField(leftHandBone, new GUIContent("Left Hand", "The left wrist bone (hand)"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
            EditorGUILayout.PropertyField(rightShoulderBone, new GUIContent("Right Shoulder", "The right shoulder bone (upper arm)"));
            EditorGUILayout.PropertyField(rightElbowBone, new GUIContent("Right Elbow", "The right elbow bone (forearm)"));
            EditorGUILayout.PropertyField(rightHandBone, new GUIContent("Right Hand", "The right wrist bone (hand)"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Legs", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
        EditorGUILayout.PropertyField(leftHipBone, new GUIContent("Left Hip", "The left hip bone (thigh)"));
        EditorGUILayout.PropertyField(leftKneeBone, new GUIContent("Left Knee", "The left knee bone (shin)"));
        EditorGUILayout.PropertyField(leftFootBone, new GUIContent("Left Foot", "The left ankle bone (foot)"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
        EditorGUILayout.PropertyField(rightHipBone, new GUIContent("Right Hip", "The right hip bone (thigh)"));
        EditorGUILayout.PropertyField(rightKneeBone, new GUIContent("Right Knee", "The right knee bone (shin)"));
        EditorGUILayout.PropertyField(rightFootBone, new GUIContent("Right Foot", "The right ankle bone (foot)"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Tweaking", EditorStyles.boldLabel);
        GUI.enabled = scaleHierarchicalModelBones.boolValue;
        EditorGUILayout.PropertyField(maxScaleFactor, new GUIContent("Maximum Scale Factor", "The maximum amount the scale of a bone can change per second when using hierarchical model bone scaling"));
        GUI.enabled = true;
        EditorGUILayout.PropertyField(minimumConfidenceToUpdate, new GUIContent("Minimum Confidence to Update", "The minimum confidence in joint positions and rotations needed to update relevant values"));
        EditorGUILayout.PropertyField(rotationDamping, new GUIContent("Max Joint Angular Velocity", "Maximum joint angular velocity can be used for damping character bone movement (smaller values)"));
        EditorGUILayout.PropertyField(neckHeightTweaker, new GUIContent("Neck Height Tweaker", "The height offset for the neck"));

		GUI.enabled = useHierarchicalModel.boolValue;
		EditorGUILayout.PropertyField(forearmLengthTweaker, new GUIContent("Forearm Length Tweaker", "The forearm length ratio compared to the real-world value, use this to lengthen or shorten the forearm. Only used for hierarchical models"));
		GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }
}
