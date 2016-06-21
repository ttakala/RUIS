/*****************************************************************************

Content    :   The base class for all wands
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2016 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections;

public abstract class RUISWand : MonoBehaviour {
    public abstract bool SelectionButtonWasPressed();
    public abstract bool SelectionButtonWasReleased();
    public abstract bool SelectionButtonIsDown();

    //returns true if the button is a standard press/release button, or false if the button is some kind of trigger type without an explicit release event (such as a skeletonwand)
    public abstract bool IsSelectionButtonStandard();

    public abstract Vector3 GetAngularVelocity();

    public virtual Color color { get; set;} 
}
