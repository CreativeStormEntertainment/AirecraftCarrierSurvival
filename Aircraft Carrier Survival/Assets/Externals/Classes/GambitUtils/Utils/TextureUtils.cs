using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace GambitUtils
{
    public struct TextureUtils
    {
        /// <summary>
        /// Creates the texture filled with given color.
        /// </summary>
        public static Texture2D CreateTexture(int width, int height, Color fillColor)
        {
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result.SetPixel(x, y, fillColor);
                }
            }
            result.Apply();
            return result;
        }

        /// <summary>
        /// Converts whole texture to one sprite.
        /// </summary>
        public static Sprite CreateSpriteFromTexture(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static void LoadImageFromFilepath(string filePath, Image image)
        {
            var bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
            texture.LoadImage(bytes);
            image.sprite = CreateSpriteFromTexture(texture);
        }

        public static IEnumerator CaptureScreenshotWithoutCanvasRoutine(Camera camera, Canvas canvas, System.Action<Texture2D> resultCallback)
        {
            bool canvasEnabled = canvas.enabled;
            canvas.enabled = false;
            yield return new WaitForEndOfFrame();
            Texture2D texture = CaptureScreenshotByRenderingToTexture(camera, TextureFormat.ARGB32);
            canvas.enabled = canvasEnabled;
            if (resultCallback != null)
            {
                resultCallback.Invoke(texture);
            }
        }

        public static Texture2D CaptureScreenshotByRenderingToTexture(Camera camera, TextureFormat textureFormat, System.Action<Texture2D> postprocess = null)
        {
            int width = Screen.width;
            int height = Screen.height;
            return CaptureScreenshotByRenderingToTexture(camera, textureFormat, width, height, postprocess);
        }

        /// <summary>
        /// /// <summary>Take screenshot using RenderTexture and Camera</summary>
        /// </summary>
        /// <param name="resultCallback"></param>
        /// <returns></returns>
        public static Texture2D CaptureScreenshotByRenderingToTexture(Camera camera, TextureFormat textureFormat, int width, int height, System.Action<Texture2D> postprocess = null)
        {
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) {
                // TODO: Customize anti-aliasing value. Anti-aliasing value must be one of (1, 2, 4 or 8), indicating the number of samples per pixel.
                antiAliasing = 4
            };

            RenderTexture target = camera.targetTexture;
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = target;

            Texture2D texture = renderTexture.ToTexture2D(textureFormat, true);
            if (postprocess != null)
            {
                postprocess?.Invoke(texture);
                texture.Apply();
            }

            return texture;
        }

        /// <summary>
        /// WARNING: ScreenCapture.CaptureScreenshotAsTexture does not work on iOS with OpenGLES, it works only with metal
        /// </summary>
        /// <param name="resultCallback"></param>
        /// <returns></returns>
        public static IEnumerator CaptureScreenshotAsTextureRoutine(System.Action<Texture2D> resultCallback)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (resultCallback != null)
            {
                resultCallback.Invoke(texture);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="spriteName"></param>
        public static void CaptureScreenshotToImage(Camera camera, Image image, string spriteName = null)
        {
            if (image.sprite)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(image.sprite);
                }
                else
                {
                    Object.DestroyImmediate(image.sprite);
                }
            }
            image.sprite = CreateSpriteFromTexture(CaptureScreenshotByRenderingToTexture(camera, TextureFormat.RGB24));
            if (!string.IsNullOrEmpty(spriteName))
            {
                image.sprite.name = spriteName;
            }
        }

        public static void SaveTextureToPNG(Texture2D texture, string filepath)
        {
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filepath, bytes);
        }

        public static void SaveTextureToPNG(Texture2D texture, string folderPath, string filename)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            SaveTextureToPNG(texture, Path.Combine(folderPath, filename));
        }

        public static void CaptureScreenshotToPNG(Camera camera, TextureFormat textureFormat, int width, int height, string folderPath, string filename, System.Action<Texture2D> postprocess = null)
        {
            var texture = CaptureScreenshotByRenderingToTexture(camera, textureFormat, width, height, postprocess);
            SaveTextureToPNG(texture, folderPath, filename);
        }

        public static void SetPixelsAlphaFromClosestPixels(Texture2D texture, byte cutoff = 0)
        {
            var pixels = texture.GetPixels32();
            if (cutoff > 0)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].a <= cutoff)
                    {
                        pixels[i].a = 0;
                    }
                }
            }
            int width = texture.width;
            int height = texture.height;
            for (int y = 1; y < height - 1; y++)
            {
                int yOffset = y * width;
                for (int x = 1; x < width - 1; x++)
                {
                    int index = yOffset + x;
                    if (pixels[index].a > 0)
                    {
                        //int opaqueNgbrCount = 0;
                        //for (int i = 0; i < 2; i++)
                        //{
                        //    for (int j = -1; j < 2; j += 2)
                        //    {
                        //        int dx = i * j;
                        //        int dy = (1 - i) * j;
                        //        int ngbrIndex = x + dx + (y + dy) * width;
                        //        opaqueNgbrCount += pixels[ngbrIndex].a > 0 ? 1 : 0;
                        //    }
                        //}
                        //pixels[index].a = (byte)((opaqueNgbrCount * 255) >> 2);
                        int opaqueNgbrCount = 0;
                        for (int dy = -1; dy < 2; dy++)
                        {
                            int ngbrOffsetY = (y + dy) * width;
                            for (int dx = -1; dx < 2; dx++)
                            {
                                int ngbrIndex = x + dx + ngbrOffsetY;
                                opaqueNgbrCount += pixels[ngbrIndex].a > 0 ? 1 : 0;
                            }
                        }
                        opaqueNgbrCount--;
                        pixels[index].a = (byte)((opaqueNgbrCount * opaqueNgbrCount * 255) >> 6);
                    }
                }
            }
            texture.SetPixels32(pixels);
        }

        public static bool ColorHasRGB(Color32 color)
        {
            return color.r > 0 || color.g > 0 || color.b > 0;
        }
    }
}
