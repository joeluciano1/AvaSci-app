using System.Collections;
using System.Collections.Generic;
using Syncfusion.Drawing;
using UnityEngine;

public static class StaticGeneralManager 
{
    public static PointF ToPointF(this Vector2 vector2)
    {
        return new PointF(vector2.x, vector2.y);
    }

    public static SizeF ToSizeF(this Vector2 vector2)
    {
        return new SizeF(vector2.x, vector2.y);
    }
}
