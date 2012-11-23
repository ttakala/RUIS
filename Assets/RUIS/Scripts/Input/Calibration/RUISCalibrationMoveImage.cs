using UnityEngine;
using System.Collections;

public class RUISCalibrationMoveImage : MonoBehaviour {
    PSMoveWrapper psMoveWrapper;

    Texture2D texture;

    void Awake()
    {
        RUISM2KCalibration calibration = FindObjectOfType(typeof(RUISM2KCalibration)) as RUISM2KCalibration;
        if (!calibration.usePSMove)
            gameObject.SetActiveRecursively(false);

        psMoveWrapper = FindObjectOfType(typeof(PSMoveWrapper)) as PSMoveWrapper;
        texture = new Texture2D(640, 480, TextureFormat.ARGB32, false);
    }
	
	// Update is called once per frame
	void Update () {
        Color32[] image = psMoveWrapper.GetCameraImage();
        if (image != null && image.Length == 640 * 480)
        {
            texture.SetPixels32(image);
            texture.Apply(false);
        }
	}

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, Screen.height/2+1, Screen.width/2, Screen.height/2), texture);
    }
}