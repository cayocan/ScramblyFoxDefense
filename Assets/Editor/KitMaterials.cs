using System.IO;
using UnityEditor;
using UnityEngine;

namespace ScramblyFoxDefense.EditorTools
{
    /// <summary>
    /// Shared Scrambly/KitLit materials for the two Kenney kits. Instances get these instead of the
    /// glTFast PBR materials, which drag a heavy shader and the skybox cubemap into the build.
    /// </summary>
    public static class KitMaterials
    {
        const string Folder = "Assets/Art/Materials";
        const string ShaderName = "Scrambly/KitLit";

        public static Material Pets => GetOrCreate("PetKit", "Assets/Art/CubePets/Textures/colormap.png");
        public static Material Towers => GetOrCreate("TowerKit", "Assets/Art/TowerDefense/Textures/colormap.png");

        public static Material ForAsset(string assetPath) =>
            assetPath.Contains("/CubePets/") ? Pets : Towers;

        public static void Apply(GameObject root, Material material)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++) materials[i] = material;
                renderer.sharedMaterials = materials;
            }
        }

        static Material GetOrCreate(string name, string texturePath)
        {
            string path = $"{Folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            Directory.CreateDirectory(Folder);
            material = new Material(Shader.Find(ShaderName))
            {
                mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath)
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
