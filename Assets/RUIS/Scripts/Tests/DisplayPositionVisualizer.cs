using UnityEngine;
using System.Collections;

public class DisplayPositionVisualizer : MonoBehaviour {
    public RUISDisplay display;
    public Material visualizerMaterial;

    void OnPostRender()
    {
        GL.PushMatrix();
            visualizerMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
                GL.Vertex(display.BottomRightPosition);
                GL.Vertex(display.TopRightPosition);
                GL.Vertex(display.TopLeftPosition);
                GL.Vertex(display.BottomLeftPosition);
            GL.End();
        GL.PopMatrix();
    }
}
