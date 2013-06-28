/*****************************************************************************

Content    :   A class to disable editing of the RUISRiftCamera class
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISRiftCamera))]
[CanEditMultipleObjects]
public class RUISRiftCameraEditor :  Editor {
    public override void OnInspectorGUI()
    {
    }
}
