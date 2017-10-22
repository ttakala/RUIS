/*****************************************************************************

Content    :   Visualize the difference between device1 and device2 calibration results
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISSampleDifferenceVisualizer : MonoBehaviour {
    public GameObject device2SamplePrefab;
    private LineRenderer lineRenderer;

	void Start () {
		lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(2);
	}
	
	void Update () {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, device2SamplePrefab.transform.position);
	}
}
