using UnityEditor;
using UnityEngine;

namespace ScramblyFoxDefense.EditorTools
{
    /// <summary>Import settings for the generated art (palette colormaps, UI sprites, font). Idempotent.</summary>
    public static class ArtImports
    {
        public const string FontPath = "Assets/Art/Fonts/Fredoka-SemiBold-Subset.ttf";
        public const string UiFolder = "Assets/Art/UI/";

        static readonly string[] Colormaps =
        {
            "Assets/Art/TowerDefense/Textures/colormap-scrambly.png",
            "Assets/Art/CubePets/Textures/colormap-predators.png",
            "Assets/Art/TowerDefense/Textures/colormap-deco.png"
        };

        static readonly string[] Sprites = { "rounded", "lock-closed", "lock-open", "coin", "trophy", "medal", "basket", "hand", "sound-on", "sound-off", "spark", "ring", "arrow-up" };

        public static Font Font => AssetDatabase.LoadAssetAtPath<Font>(FontPath);

        public static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(UiFolder + name + ".png");

        [MenuItem("Scrambly/Apply Art Import Settings")]
        public static void Ensure()
        {
            foreach (var path in Colormaps)
            {
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                // Same budget as the original kit textures: 256 px, no mipmaps.
                if (importer.maxTextureSize == 256 && !importer.mipmapEnabled) continue;
                importer.maxTextureSize = 256;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            foreach (var name in Sprites)
            {
                if (!(AssetImporter.GetAtPath(UiFolder + name + ".png") is TextureImporter importer)) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 1f; // 1 sprite pixel = 1 HUD reference pixel
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                // The tutorial hand's pivot is its fingertip, so it can be placed right on the target.
                settings.spriteAlignment = (int)(name == "hand" ? SpriteAlignment.TopCenter : SpriteAlignment.Center);
                importer.SetTextureSettings(settings);
                if (name == "rounded") importer.spriteBorder = new Vector4(22f, 22f, 22f, 22f); // 9-slice corners
                importer.SaveAndReimport();
            }
        }
    }
}
