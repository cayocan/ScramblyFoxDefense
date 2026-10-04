using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScramblyFoxDefense.EditorTools
{
    /// <summary>
    /// Shared Scrambly/KitLit materials. Instances get these instead of the glTFast PBR materials,
    /// which drag a heavy shader and the skybox cubemap into the build.
    /// </summary>
    public static class KitMaterials
    {
        const string Folder = "Assets/Art/Materials";
        const string ShaderName = "Scrambly/KitLit";
        const string PetTexture = "Assets/Art/CubePets/Textures/colormap.png";
        const string PredatorTexture = "Assets/Art/CubePets/Textures/colormap-predators.png";
        const string TowerTexture = "Assets/Art/TowerDefense/Textures/colormap-scrambly.png"; // palette remap of the kit colormap
        const string DecoTexture = "Assets/Art/TowerDefense/Textures/colormap-deco.png";      // autumn foliage, purple crystals


        public static Material Pets => GetOrCreate("PetKit", PetTexture, Color.white);
        public static Material Towers => GetOrCreate("TowerKit", TowerTexture, Color.white);
        // Predators read as "the other side": purple version of the pet palette (GDD section 6).
        public static Material Enemies => GetOrCreate("EnemyKit", PredatorTexture, Color.white);
        public static Material Deco => GetOrCreate("DecoKit", DecoTexture, Color.white);
        // Ground outside the play area: same palette, slightly darker so the board still reads as the arena.
        public static Material OuterGround => GetOrCreate("OuterGround", TowerTexture, new Color(0.84f, 0.77f, 0.7f));

        public static Material Tinted(string name, Color color) => GetOrCreate(name, null, color);

        public static Material ForAsset(string assetPath) =>
            assetPath.Contains("/CubePets/") ? Pets : Towers;

        public static void Apply(GameObject root, Material material)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
        }

        /// <summary>4x4 flat cubemap: a null custom reflection falls back to Unity's 0.5 MB built-in one.</summary>
        public static Cubemap FlatReflection
        {
            get
            {
                string path = $"{Folder}/FlatReflection.cubemap";
                var cubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
                if (cubemap != null) return cubemap;

                cubemap = new Cubemap(4, TextureFormat.RGBA32, false);
                var pixels = Enumerable.Repeat((Color)new Color32(0x20, 0x13, 0x38, 0xFF), 16).ToArray();
                for (int face = 0; face < 6; face++) cubemap.SetPixels(pixels, (CubemapFace)face);
                cubemap.Apply();
                Directory.CreateDirectory(Folder);
                AssetDatabase.CreateAsset(cubemap, path);
                return cubemap;
            }
        }

        static Material GetOrCreate(string name, string texturePath, Color color)
        {
            string path = $"{Folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Directory.CreateDirectory(Folder);
                material = new Material(Shader.Find(ShaderName));
                AssetDatabase.CreateAsset(material, path);
            }
            material.mainTexture = texturePath != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath) : null;
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
