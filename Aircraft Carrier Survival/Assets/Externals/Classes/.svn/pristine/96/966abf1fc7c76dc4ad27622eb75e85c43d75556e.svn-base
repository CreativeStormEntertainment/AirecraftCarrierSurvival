using UnityEngine;

namespace GambitUtils
{
    public static class TextureExtensionMethods
    {
        public static Texture2D ToTexture2D(this RenderTexture renderTexture, TextureFormat textureFormat, bool destroyRenderTexture)
        {
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, textureFormat, false);
            RenderTexture activePreviously = RenderTexture.active;
            RenderTexture.active = renderTexture;
            // Read screen contents into the texture
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = activePreviously;
            if (destroyRenderTexture)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(renderTexture);
                }
                else
                {
                    Object.DestroyImmediate(renderTexture);
                }
            }
            texture.name = "ScreenshotTexture";
            return texture;
        }


        public static Texture2D CopyTexture(this Texture2D texture)
        {
            var textureCopy = new Texture2D(texture.width, texture.height);
            textureCopy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            return textureCopy;
        }

        public static bool SetPixelColor(this Texture2D texture, int x, int y, TexArrayColor color, int texArrayLength, out Vector2 colorUV, out Vector2 controlUV, bool applyToTexture)
        {
            if (texture.SetPixelColor(x, y, color.Color, out colorUV, false) &&
                texture.SetPixelColor(x, y + 1, color.GetControlColor(texArrayLength), out controlUV, false))
            {
                if (applyToTexture)
                {
                    texture.Apply();
                }
                return true;
            }
            controlUV = default;
            return false;
        }

        public static bool SetPixelColor(this Texture2D texture, int x, int y, Color color, out Vector2 uv, bool applyToTexture)
        {
            if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
            {
                texture.SetPixel(x, y, color);
                if (applyToTexture)
                {
                    texture.Apply();
                }
                float u = (x + 0.5f) / texture.width;
                float v = (y + 0.5f) / texture.height;
                uv = new Vector2(u, v);
                return true;
            }
            else
            {
                Debug.LogError("TextureExtensionMethods.SetPixelColor: out of texture bounds: " + x + ", " + y);
                uv = Vector2.zero;
                return false;
            }
        }

        /// <summary>
        /// Scales the texture data of the given texture.
        /// </summary>
        /// <param name="tex">Texure to scale</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="mode">Filtering mode</param>
        public static void Scale(this Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            ScaleByDrawToRenderTexture(tex, width, height, mode);

            // Update new texture
            tex.Resize(width, height);
            tex.ReadPixels(texR, 0, 0, true);
            tex.Apply(true);    //Remove this if you hate us applying textures for you :)
        }

        /// <summary>
        ///	Returns a scaled copy of given texture. 
        /// </summary>
        /// <param name="tex">Source texure to scale</param>
        /// <param name="width">Destination texture width</param>
        /// <param name="height">Destination texture height</param>
        /// <param name="mode">Filtering mode</param>
        public static Texture2D CreateScaledTexture2D(this Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            ScaleByDrawToRenderTexture(src, width, height, mode);

            //Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Resize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }

        /// <summary>
        /// Internal utility that renders the source texture into the RTT - the scaling method itself.
        /// 		
        /// Scale is performed on the GPU using RTT, so it's blazing fast.
        /// Setting up and Getting back the texture data is the bottleneck. 
        /// But Scaling itself costs only 1 draw call and 1 RTT State setup!
        /// WARNING: This script override the RTT Setup! (It sets a RTT!)     
        ///
        /// Note: This scaler does NOT support aspect ratio based scaling. You will have to do it yourself!
        /// It supports Alpha, but you will have to divide by alpha in your shaders, 
        /// because of premultiplied alpha effect. Or you should use blend modes.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="fmode"></param>
        private static void ScaleByDrawToRenderTexture(Texture2D src, int width, int height, FilterMode fmode)
        {
            //We need the source texture in VRAM because we render with it
            //src.filterMode = fmode;
            //src.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new RenderTexture(width, height, 32);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
    }
}
