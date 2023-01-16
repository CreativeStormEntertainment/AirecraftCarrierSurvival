using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Texture2D LandMask => landMask;

    [NonSerialized]
    public List<Vector2> Nodes = null;
    [SerializeField] 
    protected Texture2D landMask = null;

    public static bool IsOnLand(Vector2 a, Texture2D landMask, float maskScale)
    {
        a *= maskScale;
        a.x += landMask.width / 2f;
        a.y += landMask.height / 2f;

        return PlotLine(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), color => color.r, landMask) != -1f;
    }

    public static bool IsOnLand2(Vector2 a, Texture2D landMask, float maskScale)
    {
        return PlotLine(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), color => color.r, landMask) != -1f;
    }

    protected static float PlotLine(int x0, int y0, int x1, int y1, Func<Color, float> getBreakColorComponent, Texture2D mask)
    {
        Vector2 start = new Vector2(x0, y0);
        Vector2 end = new Vector2(x1, y1);

        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2; /* error value e_xy */

        int limit = 0;

        for (; ; )
        {  /* loop */
            limit++;
            Color pixCol = mask.GetPixel(x0, y0);
            //TEST_landMask.sprite.texture.SetPixel(x0 - 1, y0, Color.red);
            //TEST_landMask.sprite.texture.SetPixel(x0, y0 - 1, Color.red);
            //TEST_landMask.sprite.texture.SetPixel(x0, y0, Color.red);
            //TEST_landMask.sprite.texture.SetPixel(x0 + 1, y0, Color.red);
            //TEST_landMask.sprite.texture.SetPixel(x0, y0 + 1, Color.red);
            //TEST_landMask.sprite.texture.Apply();
            if (getBreakColorComponent(pixCol) > 0)
            {
                return Utils.InverseLerp(start, end, new Vector2(x0, y0));
            }

            if (limit > 35000)
                return -1f;
            if (x0 == x1 && y0 == y1)
                break;
            e2 = 2 * err;
            if (e2 >= dy)
            { err += dy; x0 += sx; } /* e_xy+e_x > 0 */
            if (e2 <= dx)
            { err += dx; y0 += sy; } /* e_xy+e_y < 0 */

        }
        return -1f;
    }

    public bool IsOnLand(Vector2 a, float maskScale)
    {
        a *= maskScale;
        a.x += landMask.width / 2f;
        a.y += landMask.height / 2f;

        return PlotLine(Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), Mathf.RoundToInt(a.x), Mathf.RoundToInt(a.y), color => color.r, landMask) != -1f;
    }

}
