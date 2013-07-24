/*****************************************************************************

Content    :   Functionality to combine skeleton input from Kinect with a Mecanim Animator controlled avatar
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/


using UnityEngine;
using System.Collections.Generic;

public class RUISKinectAndMecanimCombiner : MonoBehaviour {
    private enum BodypartClassification
    {
        Root,
        Torso,
        Head,
        RightArm,
        LeftArm,
        RightLeg,
        LeftLeg
    }

    private class BoneTriplet
    {
        public Transform kinectTransform;
        public Transform mecanimTransform;
        public Transform blendedTransform;

        public BodypartClassification bodypartClassification;

        public List<BoneTriplet> children;

        public BoneTriplet(Transform kinectTransform, Transform mecanimTransform, Transform blendedTransform, BodypartClassification bodypartClassification)
        {
            this.kinectTransform = kinectTransform;
            this.mecanimTransform = mecanimTransform;
            this.blendedTransform = blendedTransform;

            children = new List<BoneTriplet>();

            this.bodypartClassification = bodypartClassification;
        }

        public override string ToString()
        {
            return bodypartClassification + " (" + kinectTransform.name + ", " + mecanimTransform.name + ", " + blendedTransform.name + ")";
        }
    }
    
    public Animator mecanimAnimator;
    public RUISPlainSkeletonController skeletonController;

    public float rootBlendWeight;
    public float torsoBlendWeight;
    public float headBlendWeight;
    public float rightArmBlendWeight;
    public float leftArmBlendWeight;
    public float rightLegBlendWeight;
    public float leftLegBlendWeight;

    public bool applyTorsoCounteringRotations = true;


    public bool forceArmStartPosition = true;
    public bool forceLegStartPosition = true;

    BoneTriplet skeletonRoot;
    BoneTriplet torsoRoot;
    BoneTriplet headRoot;
    BoneTriplet neckRoot;
    BoneTriplet rightArmRoot;
    BoneTriplet leftArmRoot;
    BoneTriplet rightLegRoot;
    BoneTriplet leftLegRoot;

    GameObject kinectGameObject;
    GameObject mecanimGameObject;

    private bool childrenInstantiated = false;

    void Awake()
    {
        
    }

    void Start()
    {
        /*
        */
        //start at root bone, going through all bones until we hit the torso bone
        //add all the bones encountered before the torso bone

    }

    void Update()
    {
        if (!childrenInstantiated)
        {
            childrenInstantiated = true;
            kinectGameObject = Instantiate(gameObject, transform.position, transform.rotation) as GameObject;
            kinectGameObject.name = name + "Kinect";
            kinectGameObject.transform.parent = transform.parent;
            //kinectGameObject.GetComponent<RUISKinectAndMecanimCombiner>().childrenInstantiated = true;
            Destroy(kinectGameObject.GetComponent<RUISKinectAndMecanimCombiner>());
            Destroy(kinectGameObject.GetComponent<Animator>());
            Destroy(kinectGameObject.GetComponent<RUISCharacterAnimationController>());
            foreach (Collider collider in kinectGameObject.GetComponentsInChildren<Collider>())
            {
                Destroy(collider);
            }
            foreach (Renderer meshRenderer in kinectGameObject.GetComponentsInChildren<Renderer>())
            {
                Destroy(meshRenderer);
            }


            mecanimGameObject = Instantiate(gameObject, transform.position, transform.rotation) as GameObject;
            mecanimGameObject.name = name + "Mecanim";
            mecanimGameObject.transform.parent = transform.parent;
            Destroy(mecanimGameObject.GetComponent<RUISKinectAndMecanimCombiner>());
            Destroy(mecanimGameObject.GetComponent<RUISPlainSkeletonController>());
            foreach (Collider collider in mecanimGameObject.GetComponentsInChildren<Collider>())
            {
                Destroy(collider);
            }
            foreach (Renderer meshRenderer in mecanimGameObject.GetComponentsInChildren<Renderer>())
            {
                Destroy(meshRenderer);
            }



            Destroy(GetComponent<Animator>());
            Destroy(GetComponent<RUISPlainSkeletonController>());
            Destroy(GetComponent<RUISCharacterAnimationController>());

            skeletonController = kinectGameObject.GetComponent<RUISPlainSkeletonController>();
            mecanimAnimator = mecanimGameObject.GetComponent<Animator>();

            Transform kinectRootBone = skeletonController.root;
            Transform mecanimRootBone = mecanimAnimator.transform.FindChild(kinectRootBone.name);
            Transform blendedRootBone = transform.FindChild(kinectRootBone.name);
            skeletonRoot = new BoneTriplet(kinectRootBone, mecanimRootBone, blendedRootBone, BodypartClassification.Root);

            AddChildren(ref skeletonRoot, BodypartClassification.Root);
        }
    }

    void LateUpdate()
    {
        UpdateScales(skeletonRoot);

        transform.position = kinectGameObject.transform.position;
        mecanimGameObject.transform.position = kinectGameObject.transform.position;
        
        torsoRoot.mecanimTransform.localPosition = torsoRoot.kinectTransform.localPosition;

        if (neckRoot != null)
        {
            //apply kinect neck height tweak to mecanim
            neckRoot.mecanimTransform.localPosition = neckRoot.mecanimTransform.localPosition - neckRoot.mecanimTransform.InverseTransformDirection(Vector3.up) * skeletonController.neckHeightTweaker;
        }

        Blend(skeletonRoot); 

        if (forceArmStartPosition)
        {
            rightArmRoot.blendedTransform.position = rightArmRoot.kinectTransform.position;
            //rightArmRoot.blendedTransform.rotation = rightArmRoot.kinectTransform.rotation;
            leftArmRoot.blendedTransform.position = leftArmRoot.kinectTransform.position;
            //leftArmRoot.blendedTransform.rotation = leftArmRoot.kinectTransform.rotation;
        }
        if (forceLegStartPosition)
        {
            rightLegRoot.blendedTransform.position = rightLegRoot.kinectTransform.position;
            //rightLegRoot.blendedTransform.rotation = rightLegRoot.kinectTransform.rotation;
            leftLegRoot.blendedTransform.position = leftLegRoot.kinectTransform.position;
            //leftLegRoot.blendedTransform.rotation = leftLegRoot.kinectTransform.rotation;
        }
    }

    void UpdateScales(BoneTriplet root)
    {
        root.blendedTransform.localScale = root.mecanimTransform.localScale = root.kinectTransform.localScale;

        foreach (BoneTriplet childTriplet in root.children)
        {
            UpdateScales(childTriplet);
        }
    }
    
    void Blend(BoneTriplet boneTriplet)
    {
        float blendWeight = GetBlendWeight(boneTriplet.bodypartClassification);
        boneTriplet.blendedTransform.localPosition = Vector3.Lerp(boneTriplet.kinectTransform.localPosition, boneTriplet.mecanimTransform.localPosition, blendWeight);
        boneTriplet.blendedTransform.localRotation = Quaternion.Slerp(boneTriplet.kinectTransform.localRotation, boneTriplet.mecanimTransform.localRotation, blendWeight);
        //boneTriplet.blendedTransform.localScale = boneTriplet.kinectTransform.localScale;

        if(applyTorsoCounteringRotations && IsLimbRoot(boneTriplet))
        {
            ApplyCounteringRotationToLimbRoot(torsoRoot, boneTriplet);
        }

        foreach (BoneTriplet childTriplet in boneTriplet.children)
        {
            Blend(childTriplet);
        }
    }

    private void AddChildren(ref BoneTriplet triplet, BodypartClassification parentBodypartClassification){
        for (int i = 0; i < triplet.kinectTransform.childCount; i++)
        {
            BoneTriplet childTriplet = new BoneTriplet(triplet.kinectTransform.GetChild(i),
                                                       triplet.mecanimTransform.GetChild(i),
                                                       triplet.blendedTransform.GetChild(i),
                                                       GetBodypartClassification(triplet.kinectTransform.GetChild(i), triplet.bodypartClassification));

            if (parentBodypartClassification != childTriplet.bodypartClassification)
            {
                RegisterRootBone(childTriplet);
            }

            if (childTriplet.kinectTransform == skeletonController.neck)
            {
                neckRoot = childTriplet;
            }

            triplet.children.Add(childTriplet);
            AddChildren(ref childTriplet, childTriplet.bodypartClassification);
        }
    }

    private BodypartClassification GetBodypartClassification(Transform kinectTransform, BodypartClassification parentClassification)
    {
        BodypartClassification newBodypartClassification = parentClassification;
        if (kinectTransform == skeletonController.torso)
        {
            newBodypartClassification = BodypartClassification.Torso;
        }
        else if (kinectTransform == skeletonController.head)
        {
            newBodypartClassification = BodypartClassification.Head;
        }
        else if (kinectTransform == skeletonController.rightShoulder)
        {
            newBodypartClassification = BodypartClassification.RightArm;
        }
        else if (kinectTransform == skeletonController.leftShoulder)
        {
            newBodypartClassification = BodypartClassification.LeftArm;
        }
        else if (kinectTransform == skeletonController.rightHip)
        {
            newBodypartClassification = BodypartClassification.RightLeg;
        }
        else if (kinectTransform == skeletonController.leftHip)
        {
            newBodypartClassification = BodypartClassification.LeftLeg;
        }

        return newBodypartClassification;
    }

    private float GetBlendWeight(BodypartClassification bodypartClassification)
    {
        switch (bodypartClassification)
        {
            case BodypartClassification.Root:
                return rootBlendWeight;
            case BodypartClassification.Torso:
                return torsoBlendWeight;
            case BodypartClassification.Head:
                return headBlendWeight;
            case BodypartClassification.RightArm:
                return rightArmBlendWeight;
            case BodypartClassification.LeftArm:
                return leftArmBlendWeight;
            case BodypartClassification.RightLeg:
                return rightLegBlendWeight;
            case BodypartClassification.LeftLeg:
                return leftLegBlendWeight;
            default:
                return 0;
        }
    }

    private void RegisterRootBone(BoneTriplet triplet)
    {
        switch (triplet.bodypartClassification)
        {
            case BodypartClassification.Torso:
                torsoRoot = triplet;
                return;
            case BodypartClassification.Head:
                headRoot = triplet;
                return;
            case BodypartClassification.RightArm:
                rightArmRoot = triplet;
                return;
            case BodypartClassification.LeftArm:
                leftArmRoot = triplet;
                return;
            case BodypartClassification.RightLeg:
                rightLegRoot = triplet;
                return;
            case BodypartClassification.LeftLeg:
                leftLegRoot = triplet;
                return;
        }
    }

    private void ApplyCounteringRotationToLimbRoot(BoneTriplet torsoBone, BoneTriplet limbRootBone)
    {
        //boneTriplet.blendedTransform.localRotation = Quaternion.Slerp(boneTriplet.kinectTransform.localRotation, boneTriplet.mecanimTransform.localRotation, blendWeight);

        float limbRootBlendWeight = GetBlendWeight(limbRootBone.bodypartClassification);
        
        //first set the global rotation so that it matches
        limbRootBone.blendedTransform.rotation = Quaternion.Slerp(limbRootBone.kinectTransform.rotation, limbRootBone.mecanimTransform.rotation, limbRootBlendWeight);

        //then apply the yaw to turn the limb to the same general direction as the torso
        Quaternion kinectToMecanimYaw = CalculateKinectToMecanimYaw();
        kinectToMecanimYaw = Quaternion.Slerp(Quaternion.identity, kinectToMecanimYaw, limbRootBlendWeight);

        float angles = Vector3.Angle(Vector3.up, kinectToMecanimYaw * Vector3.up);
            
        limbRootBone.blendedTransform.rotation = limbRootBone.blendedTransform.rotation * Quaternion.AngleAxis(angles, limbRootBone.blendedTransform.InverseTransformDirection(transform.up));//* newLocalRotation * Quaternion.Inverse(limbRootBone.blendedTransform.rotation) * limbRootBone.blendedTransform.rotation;
    }

    private Quaternion CalculateKinectToMecanimYaw()
    {
        Vector3 kinectTorsoForward = torsoRoot.kinectTransform.forward;
        kinectTorsoForward.y = 0;
        kinectTorsoForward.Normalize();
        Vector3 kinectTorsoUp = torsoRoot.kinectTransform.up;
        kinectTorsoUp.y = 0;
        kinectTorsoUp.Normalize();
        Quaternion kinectTorsoRotation = Quaternion.LookRotation(kinectTorsoForward, kinectTorsoUp);

        Vector3 mecanimTorsoForward = -transform.right;
        mecanimTorsoForward.y = 0;
        mecanimTorsoForward.Normalize();
        Vector3 mecanimTorsoUp = transform.forward;
        mecanimTorsoUp.y = 0;
        mecanimTorsoUp.Normalize();
        Quaternion mecanimTorsoRotation = Quaternion.LookRotation(mecanimTorsoForward, mecanimTorsoUp);

        Quaternion torsoYawRotation = Quaternion.Inverse(kinectTorsoRotation) * mecanimTorsoRotation;
        return torsoYawRotation;
    }

    private bool IsLimbRoot(BoneTriplet bone)
    {
        return bone == headRoot || bone == rightArmRoot || bone == leftArmRoot || bone == rightLegRoot || bone == leftLegRoot;
    }

    private bool IsPartOfLimb(BoneTriplet bone)
    {
        return bone.bodypartClassification == BodypartClassification.RightLeg ||
               bone.bodypartClassification == BodypartClassification.LeftLeg;
    }
}
