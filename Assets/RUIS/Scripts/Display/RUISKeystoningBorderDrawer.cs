using UnityEngine;
using System.Collections;

public class RUISKeystoningBorderDrawer : MonoBehaviour {
    RUISKeystoningConfiguration keystoningConfigurator;
    RUISKeystoning.KeystoningCorners corners;

    public enum WhichCamera
    {
        Center,
        Left,
        Right
    }

    public WhichCamera whichCamera = WhichCamera.Center;

    public Material keystoneGrid;
    public Material keystoneBorder;

	void Start () {
        keystoningConfigurator = GetComponent<RUISKeystoningConfiguration>();
        if (!keystoningConfigurator)
        {
            keystoningConfigurator = transform.parent.GetComponent<RUISKeystoningConfiguration>();
        }
	}

	void Update () {
        switch (whichCamera)
        {
            case WhichCamera.Center:
                corners = keystoningConfigurator.centerCameraCorners;
                break;
            case WhichCamera.Left:
                corners = keystoningConfigurator.leftCameraCorners;
                break;
            case WhichCamera.Right:
                corners = keystoningConfigurator.rightCameraCorners;
                break;
        }
	}

    void OnPostRender()
    {
        if (!keystoneBorder)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }

        if (keystoningConfigurator.drawKeystoningGrid)
        {
            keystoneGrid.SetPass(0);
            //GL.Clear(true, true, Color.black);

            int tileAmount = 10;
            for (int i = 0; i < tileAmount; i++)
            {
                for (int j = 0; j < tileAmount; j++)
                {
                    GL.Begin(GL.QUADS);


                    GL.TexCoord2(0, 0);

                    Vector3 leftEdgeTop = corners[0] + ((float)j / tileAmount) * (corners[3] - corners[0]);
                    Vector3 leftEdgeBottom = corners[0] + ((float)(j + 1) / tileAmount) * (corners[3] - corners[0]);
                    Vector3 rightEdgeTop = corners[1] + ((float)j / tileAmount) * (corners[2] - corners[1]);
                    Vector3 rightEdgeBottom = corners[1] + ((float)(j + 1) / tileAmount) * (corners[2] - corners[1]);

                    Vector3 topLeftPos = leftEdgeTop + (float)i / tileAmount * (rightEdgeTop - leftEdgeTop);
                    Vector3 topRightPos = leftEdgeTop + (float)(i + 1) / tileAmount * (rightEdgeTop - leftEdgeTop);
                    Vector3 bottomLeftPos = leftEdgeBottom + (float)i / tileAmount * (rightEdgeBottom - leftEdgeBottom);
                    Vector3 bottomRightPos = leftEdgeBottom + (float)(i + 1) / tileAmount * (rightEdgeBottom - leftEdgeBottom);

                    topLeftPos.z = camera.farClipPlane * 0.5f;
                    topRightPos.z = topLeftPos.z;
                    bottomLeftPos.z = topLeftPos.z;
                    bottomRightPos.z = topLeftPos.z;

                    Vector3 topLeftVertex = camera.ViewportToWorldPoint(topLeftPos);
                    GL.Vertex3(topLeftVertex.x, topLeftVertex.y, topLeftVertex.z);
                    GL.TexCoord2(1, 0);
                    Vector3 topRightVertex = camera.ViewportToWorldPoint(topRightPos);
                    GL.Vertex3(topRightVertex.x, topRightVertex.y, topLeftVertex.z);
                    GL.TexCoord2(1, 1);
                    Vector3 bottomRightVertex = camera.ViewportToWorldPoint(bottomRightPos);
                    GL.Vertex3(bottomRightVertex.x, bottomRightVertex.y, bottomRightVertex.z);
                    GL.TexCoord2(0, 1);
                    Vector3 bottomLeftVertex = camera.ViewportToWorldPoint(bottomLeftPos);
                    GL.Vertex3(bottomLeftVertex.x, bottomLeftVertex.y, bottomLeftVertex.z);
                    GL.End();
                }
            }
        }
        GL.PushMatrix();
            keystoneBorder.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.TRIANGLE_STRIP);
                GL.Vertex3(-0.02f, -0.02f, 0);
                DrawVertex(corners[3]);
                GL.Vertex3(corners[3].x, -0.02f, 0);
                DrawVertex(corners[2]);
                GL.Vertex3(corners[2].x, -0.02f, 0);
                GL.Vertex3(1.02f, -0.02f, 0);
            GL.End();

            GL.Begin(GL.TRIANGLE_STRIP);
                GL.Vertex3(-0.02f, 1.02f, 0);
                DrawVertex(corners[0]);
                GL.Vertex3(-0.02f, corners[0].y, 0);
                DrawVertex(corners[3]);
                GL.Vertex3(-0.02f, corners[3].y, 0);
                GL.Vertex3(-0.02f, -0.02f, 0);
            GL.End();

            GL.Begin(GL.TRIANGLE_STRIP);
                GL.Vertex3(1.02f, 1.02f, 0);
                DrawVertex(corners[1]);
                GL.Vertex3(corners[1].x, 1.02f, 0);
                DrawVertex(corners[0]);
                GL.Vertex3(corners[0].x, 1.02f, 0);
                GL.Vertex3(-0.02f, 1.02f, 0);
            GL.End();

            GL.Begin(GL.TRIANGLE_STRIP);
                GL.Vertex3(1.02f, -0.02f, 0);
                DrawVertex(corners[2]);
                GL.Vertex3(1.02f, corners[2].y, 0);
                DrawVertex(corners[1]);
                GL.Vertex3(1.02f, corners[1].y, 0);
                GL.Vertex3(1.02f, 1.02f, 0);
            GL.End();

        GL.PopMatrix();
    }

    public void DrawVertex(Vector2 corner)
    {
        GL.Vertex3(corner.x, corner.y, 0);
    }
}
