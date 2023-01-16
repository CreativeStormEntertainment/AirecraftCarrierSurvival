using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GambitUtils
{
    public static class TextureEditor
    {
        public static void SliceTextureToSpriteSheet(string texturePath, int width, int height, int colsCount, int rowsCount, bool inverseVertical = false)
        {
            var textureImporter = TextureImporter.GetAtPath(texturePath) as TextureImporter;
            if (!textureImporter)
            {
                Debug.Log("SliceTextureToSpriteSheet: file does not exists: " + texturePath);
                return;
            }
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Multiple;
            textureImporter.textureShape = TextureImporterShape.Texture2D;
            textureImporter.isReadable = true;
            textureImporter.SaveAndReimport();

            var textureSettings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(textureSettings);
            textureImporter.maxTextureSize = Mathf.Max(width, height);
            textureSettings.spritePixelsPerUnit = 100;
            textureSettings.spriteMeshType = SpriteMeshType.Tight;
            textureSettings.spriteExtrude = 0;
            textureImporter.SetTextureSettings(textureSettings);

            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(texturePath);
            float spriteWidth = 1f * width / colsCount;
            float spriteHeight = 1f * height / rowsCount;
            var spriteMetas = new List<SpriteMetaData>();
            int rectCount = 0;
            for (int i = 0; i < rowsCount; i++)
            {
                int y = inverseVertical ? rowsCount - i - 1 : i;
                for (int x = 0; x < colsCount; x++)
                {
                    Rect rect = new Rect(x * spriteWidth, y * spriteHeight, spriteWidth, spriteHeight);
                    var meta = new SpriteMetaData {
                        pivot = 0.5f * Vector2.one,
                        alignment = (int)SpriteAlignment.Center,
                        rect = rect,
                        name = filenameWithoutExtension + "_" + rectCount++
                    };
                    spriteMetas.Add(meta);
                }
            }
            textureImporter.spritesheet = spriteMetas.ToArray();
            textureImporter.SaveAndReimport();

            //AssetDatabase.ImportAsset(OutputFilePath, ImportAssetOptions.ForceUpdate);
        }

        public static List<Sprite> GetSprites(string spriteSheetPath)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath).OfType<Sprite>().ToList();

            sprites.Sort((a, b) => {
                var aComps = a.name.Split('_');
                var bComps = b.name.Split('_');
                if (int.TryParse(aComps[aComps.Length - 1], out int aIndex) &&
                    int.TryParse(bComps[bComps.Length - 1], out int bIndex))
                {
                    return aIndex.CompareTo(bIndex);
                }
                return 0;
            });
            return sprites;
        }
    }
}
