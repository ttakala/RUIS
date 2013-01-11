using UnityEngine;
using System.Collections;

public class WorldToScreenPointsTest : MonoBehaviour {
    public Texture texture;
    public Transform transformToDraw;
    public RUISDisplayManager displayManager;

    void Start()
    {
    }

    void OnGUI()
    {
        foreach (RUISDisplayManager.ScreenPoint point in displayManager.WorldPointToScreenPoints(transformToDraw.position))
        {
            RUISGUI.DrawTextureViewportSafe(new Rect(point.coordinates.x-15, point.coordinates.y-15, 30, 30), point.camera, texture);
            //GUI.DrawTexture(new Rect(point.x-15, point.y-15, 30, 30), texture);
        }
    }
}
