using UnityEngine;
using System.Collections;

//a bunch of GUI functions that are supposed to be called in OnGUI
public class RUISGUI {
    public static void DrawTextureViewportSafe(Rect where, Camera camera, Texture texture, bool flipYCoordinate = true)
    {
        if (!camera)
        {
            Debug.Log("null camera! " + where);
            return;
        }

        Rect viewport = camera.pixelRect;
        //Debug.Log("viewport: " + camera.name + ": " + viewport);

        
        //Debug.Log(camera.name + ": " + viewport);
        //Debug.Log("where: " + where);
        /*Debug.Log("xmax vs x: " + where.xMax + " " + viewport.x);
        Debug.Log("ymax vs y: " + where.yMax + " " + viewport.y);
        Debug.Log("x vs xmax: " + where.x + " " + viewport.xMax);
        Debug.Log("y vs ymax: " + where.y + " " + viewport.yMax);
        */
        //is the where rect inside the viewport at all?
        if (where.xMax < viewport.x ||where.x > viewport.xMax ||
            where.yMax < viewport.y || where.y > viewport.yMax)
        {
            return;
        }

        Rect newWhere;
        if (flipYCoordinate)
        {
            newWhere = Rect.MinMaxRect(Mathf.Max(where.x, viewport.x), Mathf.Max(where.y, viewport.y),
                                        Mathf.Min(where.xMax, viewport.xMax), Mathf.Min(where.yMax, viewport.yMax));
            newWhere.y = Screen.height - newWhere.y - newWhere.height;
            where.y = Screen.height - where.y - where.height;
        }
        else
        {
            newWhere = Rect.MinMaxRect(Mathf.Max(where.x, viewport.x), Mathf.Max(where.y, viewport.y),
                                        Mathf.Min(where.xMax, viewport.xMax), Mathf.Min(where.yMax, viewport.yMax));
        }
        
        


        
        //Debug.Log("newWhere: " + newWhere);

        //Debug.Log("newWhere: " + newWhere);

        //figure out which part of the texture gets drawn based on the newWhere and where
        Rect texCoords; 
        if (where.width.Equals(newWhere.width) && where.height.Equals(newWhere.height))
        {
            texCoords = new Rect(0, 0, 1, 1);
        }
        else
        {
            float textureWidth = newWhere.width / where.width;
            float textureHeight = newWhere.height / where.height;
            float textureU = (newWhere.x - where.x) / where.width;
            float textureV = 1 - ((newWhere.y + newWhere.height) - (where.y + where.height)) / where.height; //since the y coordinates might be flipped, we need to do some magic
            texCoords = new Rect(textureU, textureV, textureWidth, textureHeight);
        }
        
        GUI.DrawTextureWithTexCoords(newWhere, texture, texCoords);
    }
}