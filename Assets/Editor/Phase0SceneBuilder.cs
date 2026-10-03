using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ScramblyFoxDefense.EditorTools
{
    /// <summary>
    /// Builds the Phase 0 size-test scene (GDD section 8): camera, one kit tower, one animated
    /// Cube Pet on top, one sprite and one text. UI uses SpriteRenderer + TextMesh (no uGUI/TMP,
    /// see docs/rag/decisions.md). Reproducible from the menu.
    /// </summary>
    public static class Phase0SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string SpritePath = "Assets/Art/Placeholder/fox-placeholder.png";
        const float PetScale = 0.4f;

        static readonly Color Orange = new Color32(0xF5, 0x83, 0x24, 0xFF);
        static readonly Color DeepInk = new Color32(0x20, 0x13, 0x38, 0xFF);
        static readonly Color WarmWhite = new Color32(0xFF, 0xF6, 0xE8, 0xFF);

        [MenuItem("Scrambly/Phase 0/Build Size-Test Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // No skybox or reflection probe: the default skybox bakes a 0.5 MB cubemap into the build.
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.5f, 0.6f);
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            // A null custom cubemap falls back to Unity's built-in 0.5 MB one, so point it at a 4x4 flat cubemap.
            RenderSettings.customReflectionTexture = LoadOrCreateFlatReflection();

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = DeepInk;
            camera.fieldOfView = 40f;
            cameraGo.transform.SetPositionAndRotation(new Vector3(0f, 5.5f, -3.85f), Quaternion.Euler(55f, 0f, 0f));

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var tower = Spawn("Assets/Art/TowerDefense/tower-round-base.glb", Vector3.zero);
            float towerTop = tower.GetComponentsInChildren<Renderer>().Select(r => r.bounds.max.y).Max();
            // Kit tiles are 1 unit wide; Cube Pets are ~2.3 units long, so pets are scaled down to sit on a tower.
            var fox = Spawn("Assets/Art/CubePets/animal-fox.glb", new Vector3(0f, towerTop, 0f));
            fox.transform.localScale = Vector3.one * PetScale;
            var animation = fox.GetComponent<Animation>();
            var idle = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/CubePets/animal-fox.glb")
                .OfType<AnimationClip>().First(c => c.name == "idle");
            animation.clip = idle;
            animation.playAutomatically = true;
            animation.wrapMode = WrapMode.Loop;

            BuildWorldUi(cameraGo.transform);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("[Phase0] Size-test scene saved to " + ScenePath);
        }

        static GameObject Spawn(string assetPath, Vector3 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
            KitMaterials.Apply(instance, KitMaterials.ForAsset(assetPath));
            return instance;
        }

        /// <summary>Legacy TextMesh and SpriteRenderer parented to the camera.</summary>
        static void BuildWorldUi(Transform camera)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var hud = new GameObject("World UI").transform;
            hud.SetParent(camera, false);
            hud.localPosition = new Vector3(0f, 0f, 3f);

            var label = new GameObject("Coins Text", typeof(TextMesh)).GetComponent<TextMesh>();
            label.transform.SetParent(hud, false);
            label.transform.localPosition = new Vector3(0f, 1f, 0f);
            label.font = font;
            label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            label.text = "Demo coins: 70";
            label.anchor = TextAnchor.MiddleCenter;
            label.characterSize = 0.05f;
            label.fontSize = 48;
            label.color = WarmWhite;

            var fox = new GameObject("Fox Sprite", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            fox.transform.SetParent(hud, false);
            fox.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            fox.transform.localScale = Vector3.one * 0.3f;
            fox.sprite = LoadOrCreatePlaceholderSprite();
        }

        static Cubemap LoadOrCreateFlatReflection()
        {
            const string path = "Assets/Art/Materials/FlatReflection.cubemap";
            var cubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            if (cubemap != null) return cubemap;

            cubemap = new Cubemap(4, TextureFormat.RGBA32, false);
            var pixels = Enumerable.Repeat((Color)DeepInk, 16).ToArray();
            for (int face = 0; face < 6; face++) cubemap.SetPixels(pixels, (CubemapFace)face);
            cubemap.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(cubemap, path);
            return cubemap;
        }

        static Sprite LoadOrCreatePlaceholderSprite()
        {
            if (!File.Exists(SpritePath))
            {
                const int size = 128;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var center = new Vector2(size / 2f, size / 2f);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool inside = Vector2.Distance(new Vector2(x, y), center) < size * 0.45f;
                    texture.SetPixel(x, y, inside ? (y > size * 0.55f ? Orange : WarmWhite) : Color.clear);
                }
                Directory.CreateDirectory(Path.GetDirectoryName(SpritePath));
                File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(SpritePath);

                var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }
    }
}
