using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScramblyFoxDefense.Config;
using ScramblyFoxDefense.Core;
using ScramblyFoxDefense.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ScramblyFoxDefense.EditorTools
{
    /// <summary>
    /// Builds the Main scene, prefabs and default GameConfig from code so the layout is reproducible.
    /// Board: 6 x 10 tiles, S-shaped path from the top (spawn) to the bottom (Reward Vault).
    /// </summary>
    public static class MainSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string LightingPath = "Assets/Scenes/MainLighting.lighting";
        const string PrefabFolder = "Assets/Prefabs";
        const string ConfigPath = "Assets/Config/GameConfig.asset";
        const string Kit = "Assets/Art/TowerDefense/";
        const string Pets = "Assets/Art/CubePets/";

        const int Columns = 6;
        const int Rows = 10;
        const float PetOnTowerScale = 0.4f;

        // Yaw that makes each kit tile match the path; tuned by looking at the board.
        const float StraightBaseYaw = 0f;   // tile-straight runs along +Z at yaw 0
        const float CornerBaseYaw = 270f;   // tile-corner-round joins -X and +Z at yaw 0

        // Path corners in (column, row); row 0 is the top of the screen.
        static readonly Vector2Int[] PathCorners =
        {
            new Vector2Int(1, 0), new Vector2Int(1, 3), new Vector2Int(4, 3),
            new Vector2Int(4, 6), new Vector2Int(1, 6), new Vector2Int(1, 9)
        };

        static readonly Vector2Int[] SlotCells =
        {
            new Vector2Int(2, 1), new Vector2Int(3, 4), new Vector2Int(2, 5), new Vector2Int(2, 8)
        };

        static readonly Color Orange = new Color32(0xF5, 0x83, 0x24, 0xFF);
        static readonly Color Purple = new Color32(0x78, 0x45, 0xD8, 0xFF);
        static readonly Color DeepInk = new Color32(0x20, 0x13, 0x38, 0xFF);
        static readonly Color WarmWhite = new Color32(0xFF, 0xF6, 0xE8, 0xFF);

        [MenuItem("Scrambly/Build Main Scene")]
        public static void Build()
        {
            // NewScene(Single) unloads unused assets, so create the scene before loading config and prefabs.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ArtImports.Ensure();
            var prefabs = BuildPrefabs();
            var config = LoadOrCreateConfig(prefabs);
            SetupRendering();

            var camera = CreateCamera();
            var board = new GameObject("Board").transform;
            BuildBoard(board, out var waypoints, out var slots);

            var vault = Spawn(Kit + "tower-square-bottom-a.glb", board);
            vault.name = "Reward Vault";
            vault.transform.position = CellToWorld(PathCorners[PathCorners.Length - 1]) + Vector3.back * 0.9f;
            var fox = Spawn(Pets + "animal-fox.glb", vault.transform);
            fox.transform.localPosition = Vector3.up * Top(vault);
            fox.transform.localScale = Vector3.one * PetOnTowerScale;
            fox.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            PlayIdle(fox);

            var installer = new GameObject("Game").AddComponent<GameInstaller>();
            var hud = CreateHud(camera.transform);
            var cards = CreateCards(camera.transform);
            var locks = CreateLocks(camera.transform);
            var restart = CreateButton("Restart", camera.transform, new Vector2(76f, 32f), KitMaterials.Tinted("ButtonSecondary", new Color(0.36f, 0.28f, 0.52f)), WarmWhite, 14f);
            var endCard = CreateEndCard(camera.transform);
            var mute = CreateButton("", camera.transform, new Vector2(40f, 32f), KitMaterials.Tinted("ButtonSecondary", new Color(0.36f, 0.28f, 0.52f)), WarmWhite, 14f);
            mute.root.name = "Button Mute";
            var muteIcon = Icon("Icon", mute.root, ArtImports.Sprite("sound-on"), 22f, WarmWhite);
            Wire(installer, config, camera, waypoints, slots, prefabs, hud, cards, locks);
            WireEndCard(installer, vault.transform, restart, endCard);
            var wiring = new SerializedObject(installer);
            SetButton(wiring.FindProperty("muteButton"), mute);
            wiring.FindProperty("muteIcon").objectReferenceValue = muteIcon;
            wiring.FindProperty("soundOnSprite").objectReferenceValue = ArtImports.Sprite("sound-on");
            wiring.FindProperty("soundOffSprite").objectReferenceValue = ArtImports.Sprite("sound-off");
            var hand = Icon("Tutorial Hand", camera.transform, ArtImports.Sprite("hand"), 64f, Color.white);
            hand.sortingOrder = 10; // above cards and text
            wiring.FindProperty("tutorialHand").objectReferenceValue = hand;
            wiring.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            BakeLighting();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("[MainSceneBuilder] Main scene built.");
        }

        // ---------- Board ----------

        static float BuildBoard(Transform board, out Transform[] waypoints, out Transform[] slots)
        {
            var pathCells = ExpandPath(PathCorners);
            var pathIndex = new Dictionary<Vector2Int, int>();
            for (int i = 0; i < pathCells.Count; i++) pathIndex[pathCells[i]] = i;

            float tileTop = 0f;
            for (int row = 0; row < Rows; row++)
            for (int column = 0; column < Columns; column++)
            {
                var cell = new Vector2Int(column, row);
                GameObject tile;
                if (pathIndex.TryGetValue(cell, out int i))
                {
                    tile = PathTile(pathCells, i, board);
                }
                else
                {
                    tile = Spawn(Kit + "tile.glb", board);
                }
                tile.transform.position = CellToWorld(cell);
                tileTop = Mathf.Max(tileTop, Top(tile));
            }

            Decorate(board, pathIndex, tileTop);

            var waypointRoot = new GameObject("Path").transform;
            waypointRoot.SetParent(board, false);
            var points = new List<Transform>();
            // Enter from just above the board, end at the vault.
            points.Add(Waypoint(waypointRoot, CellToWorld(PathCorners[0]) + Vector3.forward * 1f, tileTop));
            foreach (var corner in PathCorners) points.Add(Waypoint(waypointRoot, CellToWorld(corner), tileTop));
            waypoints = points.ToArray();

            var slotRoot = new GameObject("Slots").transform;
            slotRoot.SetParent(board, false);
            slots = SlotCells.Select(cell =>
            {
                var marker = Spawn(Kit + "selection-a.glb", slotRoot);
                marker.name = $"Slot {cell.x},{cell.y}";
                marker.transform.position = CellToWorld(cell) + Vector3.up * tileTop;
                KitMaterials.Apply(marker, KitMaterials.Tinted("SlotMarker", Purple));
                return marker.transform;
            }).ToArray();

            return tileTop;
        }

        /// <summary>
        /// Deterministic scatter of autumn trees, rocks and crystals on the empty cells, so the board does not
        /// read as a flat plane. Tall trees never stand in front of (camera side of) a path cell, where they
        /// would hide predators, and nothing is placed next to a build slot.
        /// </summary>
        static void Decorate(Transform board, Dictionary<Vector2Int, int> pathIndex, float tileTop)
        {
            var root = new GameObject("Decoration").transform;
            root.SetParent(board, false);
            var random = new System.Random(7);
            for (int row = 0; row < Rows; row++)
            for (int column = 0; column < Columns; column++)
            {
                var cell = new Vector2Int(column, row);
                if (pathIndex.ContainsKey(cell)) continue;
                if (SlotCells.Any(slot => Mathf.Abs(slot.x - cell.x) <= 1 && Mathf.Abs(slot.y - cell.y) <= 1)) continue;
                if (row >= Rows - 2 && column <= 2) continue; // keep the vault corner clear
                if (random.NextDouble() > 0.62) continue;

                // The cell behind on screen is row - 1 (further from the camera).
                bool shortOnly = pathIndex.ContainsKey(cell + Vector2Int.down);
                double roll = random.NextDouble();
                string model = shortOnly
                    ? (roll < 0.6 ? "detail-rocks" : "detail-crystal")
                    : (roll < 0.45 ? "detail-tree" : roll < 0.7 ? "detail-tree-large" : roll < 0.87 ? "detail-rocks" : "detail-crystal");

                var deco = Spawn(Kit + model + ".glb", root);
                KitMaterials.Apply(deco, KitMaterials.Deco);
                var offset = new Vector3((float)(random.NextDouble() - 0.5) * 0.3f, 0f, (float)(random.NextDouble() - 0.5) * 0.3f);
                deco.transform.position = CellToWorld(cell) + offset + Vector3.up * tileTop;
                deco.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
                deco.transform.localScale = Vector3.one * (0.85f + (float)random.NextDouble() * 0.3f);
            }
        }

        static GameObject PathTile(List<Vector2Int> cells, int i, Transform parent)
        {
            if (i == 0) return Rotated(Spawn(Kit + "tile-spawn.glb", parent), Yaw(cells[1] - cells[0]) + StraightBaseYaw);
            if (i == cells.Count - 1) return Rotated(Spawn(Kit + "tile-end.glb", parent), Yaw(cells[i] - cells[i - 1]) + StraightBaseYaw);

            var inDir = cells[i] - cells[i - 1];
            var outDir = cells[i + 1] - cells[i];
            if (inDir == outDir) return Rotated(Spawn(Kit + "tile-straight.glb", parent), Yaw(inDir) + StraightBaseYaw);

            // The corner joins the side we came from (-inDir) and the side we leave through (outDir).
            float a = Yaw(-inDir), b = Yaw(outDir);
            float first = Mathf.Approximately(Mathf.Repeat(b - a, 360f), 90f) ? a : b;
            return Rotated(Spawn(Kit + "tile-corner-round.glb", parent), first - CornerBaseYaw);
        }

        static List<Vector2Int> ExpandPath(Vector2Int[] corners)
        {
            var cells = new List<Vector2Int> { corners[0] };
            for (int i = 1; i < corners.Length; i++)
            {
                var step = new Vector2Int(System.Math.Sign(corners[i].x - corners[i - 1].x), System.Math.Sign(corners[i].y - corners[i - 1].y));
                var cell = corners[i - 1];
                while (cell != corners[i])
                {
                    cell += step;
                    cells.Add(cell);
                }
            }
            return cells;
        }

        /// <summary>Grid step to world yaw: row grows toward the camera (-Z), column grows to +X.</summary>
        static float Yaw(Vector2Int step) => Mathf.Atan2(step.x, -step.y) * Mathf.Rad2Deg;

        static Vector3 CellToWorld(Vector2Int cell) =>
            new Vector3(cell.x - (Columns - 1) * 0.5f, 0f, (Rows - 1) * 0.5f - cell.y);

        static Transform Waypoint(Transform parent, Vector3 position, float height)
        {
            var point = new GameObject("Waypoint").transform;
            point.SetParent(parent, false);
            point.position = position + Vector3.up * height;
            return point;
        }

        // ---------- Prefabs and config ----------

        sealed class Prefabs
        {
            public GameObject PopBlaster, PuzzlePulse, RacerZap, Lion, Tiger, Polar, Projectile, Coin, Badge, Poof;
        }

        static Prefabs BuildPrefabs()
        {
            Directory.CreateDirectory(PrefabFolder);
            return new Prefabs
            {
                PopBlaster = TowerPrefab("Tower_PopBlaster", "animal-dog"),
                PuzzlePulse = TowerPrefab("Tower_PuzzlePulse", "animal-cat"),
                RacerZap = TowerPrefab("Tower_RacerZap", "animal-fox"),
                Lion = EnemyPrefab("Enemy_Snatcher", "animal-lion", 0.35f),
                Tiger = EnemyPrefab("Enemy_Dart", "animal-tiger", 0.32f),
                Polar = EnemyPrefab("Enemy_Hauler", "animal-polar", 0.45f),
                Projectile = PrimitivePrefab("Projectile", PrimitiveType.Sphere, Vector3.one * 0.15f, Quaternion.identity, KitMaterials.Tinted("Projectile", Orange)),
                Badge = BadgePrefab(),
                Poof = PoofPrefab(),
                Coin = PrimitivePrefab("Coin", PrimitiveType.Cylinder, new Vector3(0.28f, 0.03f, 0.28f), Quaternion.Euler(90f, 0f, 0f), KitMaterials.Tinted("Coin", new Color(1f, 0.75f, 0.2f)))
            };
        }

        static GameObject TowerPrefab(string name, string pet)
        {
            var root = new GameObject(name);
            var tower = Spawn(Kit + "tower-round-base.glb", root.transform);
            tower.name = "Base"; // TowerSystem stacks copies of it on upgrade
            var petRoot = new GameObject("Pet").transform;
            petRoot.SetParent(root.transform, false);
            petRoot.localPosition = Vector3.up * Top(tower);
            var model = Spawn(Pets + pet + ".glb", petRoot);
            model.transform.localScale = Vector3.one * PetOnTowerScale;
            PlayIdle(model);
            return SavePrefab(root);
        }

        static GameObject EnemyPrefab(string name, string pet, float scale)
        {
            var root = new GameObject(name);
            var model = Spawn(Pets + pet + ".glb", root.transform);
            model.transform.localScale = Vector3.one * scale;
            KitMaterials.Apply(model, KitMaterials.Enemies);
            return SavePrefab(root);
        }

        static GameObject PoofPrefab()
        {
            var poof = Icon("Poof", null, ArtImports.Sprite("coin"), 64f, WarmWhite);
            poof.transform.localScale = Vector3.one;
            return SavePrefab(poof.gameObject);
        }

        static GameObject BadgePrefab()
        {
            var text = CreateText("Badge", null, Orange);
            text.transform.localScale = Vector3.one * 3f;
            return SavePrefab(text.gameObject);
        }

        static GameObject PrimitivePrefab(string name, PrimitiveType type, Vector3 scale, Quaternion rotation, Material material)
        {
            var root = new GameObject(name);
            var shape = GameObject.CreatePrimitive(type);
            // Physics is stripped from the build: primitives must not keep their colliders.
            Object.DestroyImmediate(shape.GetComponent<Collider>());
            shape.transform.SetParent(root.transform, false);
            shape.transform.localScale = scale;
            shape.transform.localRotation = rotation;
            shape.GetComponent<Renderer>().sharedMaterial = material;
            return SavePrefab(root);
        }

        static GameObject SavePrefab(GameObject root)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{root.name}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>
        /// Creates the config once (GDD values, tuned for the 15-unit path: health x1.5, range x0.7, slower
        /// spawns; see docs/rag/decisions.md). Later runs only refresh prefab links.
        /// </summary>
        static GameConfig LoadOrCreateConfig(Prefabs prefabs)
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
                config.towers = new[]
                {
                    new TowerDefinition { displayName = "Pop Blaster", levels = new[]
                    {
                        new TowerLevel { cost = 30, damage = 4, fireRate = 2f, range = 2.3f },
                        new TowerLevel { cost = 40, damage = 6, fireRate = 2.4f, range = 2.3f },
                        new TowerLevel { cost = 60, damage = 9, fireRate = 2.8f, range = 2.3f }
                    }},
                    new TowerDefinition { displayName = "Puzzle Pulse", levels = new[]
                    {
                        new TowerLevel { cost = 40, damage = 7, fireRate = 1f, range = 2.4f, splashRadius = 1.5f },
                        new TowerLevel { cost = 50, damage = 10, fireRate = 1f, range = 2.4f, splashRadius = 1.6f },
                        new TowerLevel { cost = 70, damage = 14, fireRate = 1.1f, range = 2.4f, splashRadius = 1.9f }
                    }},
                    new TowerDefinition { displayName = "Racer Zap", levels = new[]
                    {
                        new TowerLevel { cost = 55, damage = 16, fireRate = 0.7f, range = 3.6f },
                        new TowerLevel { cost = 60, damage = 24, fireRate = 0.75f, range = 3.8f },
                        new TowerLevel { cost = 80, damage = 34, fireRate = 0.85f, range = 4f }
                    }}
                };
                config.enemies = new[]
                {
                    new EnemyDefinition { displayName = "Snatcher", health = 15, speed = 1.6f, coinReward = 6 },
                    new EnemyDefinition { displayName = "Dart", health = 9, speed = 2.8f, coinReward = 7 },
                    new EnemyDefinition { displayName = "Hauler", health = 60, speed = 1f, coinReward = 15 }
                };
                config.waves = new[]
                {
                    new WaveDefinition { spawnInterval = 1.8f, groups = new[] { new SpawnGroup { enemyIndex = 0, count = 6 } } },
                    new WaveDefinition { spawnInterval = 1.3f, groups = new[] { new SpawnGroup { enemyIndex = 0, count = 5 }, new SpawnGroup { enemyIndex = 1, count = 5 } } },
                    new WaveDefinition { spawnInterval = 1.1f, groups = new[] { new SpawnGroup { enemyIndex = 0, count = 6 }, new SpawnGroup { enemyIndex = 1, count = 4 }, new SpawnGroup { enemyIndex = 2, count = 3 } } }
                };
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                AssetDatabase.CreateAsset(config, ConfigPath);
            }

            ApplyFeelDefaults(config);
            config.towers[0].prefab = prefabs.PopBlaster;
            config.towers[1].prefab = prefabs.PuzzlePulse;
            config.towers[2].prefab = prefabs.RacerZap;
            config.enemies[0].prefab = prefabs.Lion;
            config.enemies[1].prefab = prefabs.Tiger;
            config.enemies[2].prefab = prefabs.Polar;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return config;
        }

        /// <summary>Tower feel and wave pacing (progression pass); only fills fields that are still unset.</summary>
        static void ApplyFeelDefaults(GameConfig config)
        {
            if (string.IsNullOrEmpty(config.waves[1].intro))
            {
                config.waves[0].intro = "Lions want the coins!"; config.waves[0].clearBonus = 15;
                config.waves[1].intro = "Next: Tigers — fast!"; config.waves[1].clearBonus = 20;
                config.waves[2].intro = "Next: Polar bears — tough!";
            }
            if (config.towers[0].projectileSpeed <= 0f)
            {
                SetFeel(config.towers[0], TargetMode.First, new Color(0.96f, 0.51f, 0.14f), 0.8f, 11f);
                SetFeel(config.towers[1], TargetMode.First, new Color(0.55f, 0.35f, 0.95f), 1.6f, 6f);
                SetFeel(config.towers[2], TargetMode.Strongest, new Color(1f, 0.85f, 0.2f), 0.7f, 18f);
            }
        }

        static void SetFeel(TowerDefinition tower, TargetMode targeting, Color color, float scale, float speed)
        {
            tower.targeting = targeting;
            tower.projectileColor = color;
            tower.projectileScale = scale;
            tower.projectileSpeed = speed;
        }

        // ---------- Scene plumbing ----------

        static void SetupRendering()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.5f, 0.6f);
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = KitMaterials.FlatReflection;

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = DeepInk;
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.5f;
            go.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            go.transform.position = -go.transform.forward * 14f;
            return camera;
        }

        static TextMesh CreateText(string name, Transform parent, Color color)
        {
            var font = ArtImports.Font;
            var text = new GameObject(name, typeof(TextMesh)).GetComponent<TextMesh>();
            if (parent != null) text.transform.SetParent(parent, false);
            text.font = font;
            var textRenderer = text.GetComponent<MeshRenderer>();
            textRenderer.sharedMaterial = font.material;
            textRenderer.sortingOrder = 5; // above rounded sprite backgrounds on the HUD plane
            text.fontSize = 64;
            text.characterSize = 0.01f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = color;
            text.text = name;
            return text;
        }

        static TextMesh[] CreateHud(Transform camera)
        {
            var root = new GameObject("HUD").transform;
            root.SetParent(camera, false);
            var coin = Icon("Coin Icon", root, ArtImports.Sprite("coin"), 64f, new Color(1f, 0.78f, 0.25f));
            coin.name = "Coin Icon";
            // Top band behind the buttons, balance and locks; laid out by HudView.
            var band = Rounded("Top Band", root, new Vector2(420f, 140f), new Color(0.18f, 0.11f, 0.32f, 0.92f), -5);
            band.name = "Top Band";
            // Pill behind wave banners, resized to the text by HudView.
            var pill = Rounded("Banner Pill", root, new Vector2(200f, 44f), new Color(0.13f, 0.07f, 0.22f, 0.9f), 4);
            pill.name = "Banner Pill";
            return new[]
            {
                CreateText("Coins", root, WarmWhite),
                CreateText("Phase", root, WarmWhite),
                CreateText("Banner", root, Orange)
            };
        }

        /// <summary>Card children are laid out in reference pixels (the root is scaled by HudLayout).</summary>
        static CardView[] CreateCards(Transform camera)
        {
            var root = new GameObject("Cards").transform;
            root.SetParent(camera, false);
            var pets = new[] { "animal-dog", "animal-cat", "animal-fox" };
            return pets.Select((pet, i) =>
            {
                var card = new GameObject($"Card {i}").transform;
                card.SetParent(root, false);

                var shadow = Rounded("Shadow", card, new Vector2(112f, 128f), new Color(0f, 0f, 0f, 0.35f), -2);
                shadow.transform.localPosition = new Vector3(0f, -5f, 0f);
                var background = Rounded("Background", card, new Vector2(112f, 128f), Color.white);

                var face = Spawn(Pets + pet + ".glb", card);
                face.name = "Face";
                face.transform.localPosition = new Vector3(0f, -14f, -20f);
                face.transform.localRotation = Quaternion.Euler(-20f, 200f, 0f);
                face.transform.localScale = Vector3.one * 34f;
                PlayIdle(face);

                var title = CreateText("Title", card, DeepInk);
                title.transform.localPosition = new Vector3(0f, -38f, 0f);
                title.transform.localScale = Vector3.one * (13f / 0.064f);
                var price = CreateText("Price", card, DeepInk);
                price.transform.localPosition = new Vector3(0f, -54f, 0f);
                price.transform.localScale = Vector3.one * (13f / 0.064f);

                return new CardView { root = card, background = background, title = title, price = price };
            }).ToArray();
        }

        sealed class EndCardParts
        {
            public Transform Panel;
            public TextMesh Title, Collected, Toast;
            public Transform[] Rewards;
            public HudButton Cta, PlayAgain;
        }

        /// <summary>Final screen; children in reference pixels around the panel centre.</summary>
        static EndCardParts CreateEndCard(Transform camera)
        {
            var panel = new GameObject("End Card").transform;
            panel.SetParent(camera, false);
            Rounded("Panel", panel, new Vector2(340f, 520f), WarmWhite, -1);

            TextMesh Label(string text, float y, float height, Color color)
            {
                var label = CreateText(text, panel, color);
                label.text = text;
                label.transform.localPosition = new Vector3(0f, y, 0f);
                label.transform.localScale = Vector3.one * (height / 0.064f);
                return label;
            }

            var parts = new EndCardParts
            {
                Panel = panel,
                Title = Label("Perfect defense!", 196f, 32f, Orange),
                Collected = Label("You collected 0 demo coins", 156f, 15f, DeepInk)
            };
            Label("Discover games.\nPlay and progress.\nRedeem rewards.", 98f, 17f, DeepInk);

            var rewardNames = new[] { "Trophy", "Medal", "Basket" };
            var rewardColors = new[] { new Color(1f, 0.78f, 0.25f), Orange, Purple };
            parts.Rewards = rewardNames.Select((name, i) =>
            {
                var reward = new GameObject($"Reward {name}").transform;
                reward.SetParent(panel, false);
                reward.localPosition = new Vector3((i - 1) * 100f, 6f, 0f);
                Rounded("Card", reward, new Vector2(88f, 96f), new Color(1f, 0.9f, 0.78f));
                Icon("Icon", reward, ArtImports.Sprite(name.ToLowerInvariant()), 48f, rewardColors[i]).transform.localPosition = new Vector3(0f, 12f, 0f);
                var caption = CreateText(name, reward, DeepInk);
                caption.transform.localPosition = new Vector3(0f, -32f, 0f);
                caption.transform.localScale = Vector3.one * (13f / 0.064f);
                return reward;
            }).ToArray();

            parts.Cta = CreateButton("Explore Scrambly", panel, new Vector2(270f, 62f), KitMaterials.Tinted("ButtonPrimary", Orange), WarmWhite, 22f);
            parts.Cta.root.localPosition = new Vector3(0f, -96f, 0f);
            parts.PlayAgain = CreateButton("Play again", panel, new Vector2(170f, 42f), KitMaterials.Tinted("ButtonSecondary", new Color(0.36f, 0.28f, 0.52f)), WarmWhite, 16f);
            parts.PlayAgain.root.localPosition = new Vector3(0f, -160f, 0f);
            Label("Demo only — not real earnings", -208f, 12f, new Color(0.35f, 0.3f, 0.42f));
            parts.Toast = Label("CTA clicked — demo only", -238f, 16f, Orange);
            return parts;
        }

        static HudButton CreateButton(string text, Transform parent, Vector2 size, Material material, Color textColor, float textHeight)
        {
            var root = new GameObject($"Button {text}").transform;
            root.SetParent(parent, false);
            var background = Rounded("Background", root, size, material.color);
            var label = CreateText(text, root, textColor);
            label.transform.localScale = Vector3.one * (textHeight / 0.064f);
            return new HudButton { root = root, background = background, label = label, sizePixels = size };
        }

        static void WireEndCard(GameInstaller installer, Transform vault, HudButton restart, EndCardParts endCard)
        {
            var so = new SerializedObject(installer);
            so.FindProperty("vault").objectReferenceValue = vault;
            so.FindProperty("endCardPanel").objectReferenceValue = endCard.Panel;
            so.FindProperty("endTitle").objectReferenceValue = endCard.Title;
            so.FindProperty("endCollected").objectReferenceValue = endCard.Collected;
            so.FindProperty("ctaToast").objectReferenceValue = endCard.Toast;
            SetArray(so.FindProperty("rewardCards"), endCard.Rewards);
            SetButton(so.FindProperty("restartButton"), restart);
            SetButton(so.FindProperty("ctaButton"), endCard.Cta);
            SetButton(so.FindProperty("playAgainButton"), endCard.PlayAgain);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetButton(SerializedProperty property, HudButton button)
        {
            property.FindPropertyRelative("root").objectReferenceValue = button.root;
            property.FindPropertyRelative("background").objectReferenceValue = button.background;
            property.FindPropertyRelative("label").objectReferenceValue = button.label;
            property.FindPropertyRelative("sizePixels").vector2Value = button.sizePixels;
        }

        static SpriteRenderer[] CreateLocks(Transform camera)
        {
            var root = new GameObject("Locks").transform;
            root.SetParent(camera, false);
            return Enumerable.Range(0, 3)
                .Select(i => Icon($"Lock {i + 1}", root, ArtImports.Sprite("lock-closed"), 64f, Color.white))
                .ToArray();
        }

        /// <summary>9-sliced rounded rectangle (the brief asks for rounded, warm shapes); size in reference pixels.</summary>
        static SpriteRenderer Rounded(string name, Transform parent, Vector2 size, Color color, int sortingOrder = 0)
        {
            var sprite = new GameObject(name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            sprite.transform.SetParent(parent, false);
            sprite.sprite = ArtImports.Sprite("rounded");
            sprite.drawMode = SpriteDrawMode.Sliced;
            sprite.size = size;
            sprite.color = color;
            sprite.sortingOrder = sortingOrder;
            return sprite;
        }

        static SpriteRenderer Icon(string name, Transform parent, Sprite icon, float sizePixels, Color color)
        {
            var sprite = new GameObject(name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            sprite.transform.SetParent(parent, false);
            sprite.sprite = icon;
            sprite.color = color;
            sprite.sortingOrder = 1;
            sprite.transform.localScale = Vector3.one * (sizePixels / 64f);
            return sprite;
        }

        static Renderer Quad(string name, Transform parent, Vector3 scale, Material material)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(quad.GetComponent<Collider>()); // physics is stripped from the build
            quad.name = name;
            quad.transform.SetParent(parent, false);
            quad.transform.localScale = scale;
            var renderer = quad.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            return renderer;
        }

        static void Wire(GameInstaller installer, GameConfig config, Camera camera, Transform[] waypoints, Transform[] slots, Prefabs prefabs, TextMesh[] hud, CardView[] cards, SpriteRenderer[] locks)
        {
            var roots = new[] { "Enemies", "Towers", "Fx" }.Select(n => new GameObject(n).transform).ToArray();
            var so = new SerializedObject(installer);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("mainCamera").objectReferenceValue = camera;
            SetArray(so.FindProperty("pathWaypoints"), waypoints);
            SetArray(so.FindProperty("slotTransforms"), slots);
            so.FindProperty("boardSize").vector2Value = new Vector2(Columns, Rows + 1); // + vault row
            so.FindProperty("projectilePrefab").objectReferenceValue = prefabs.Projectile;
            so.FindProperty("coinPrefab").objectReferenceValue = prefabs.Coin;
            so.FindProperty("badgePrefab").objectReferenceValue = prefabs.Badge;
            so.FindProperty("poofPrefab").objectReferenceValue = prefabs.Poof;
            so.FindProperty("sparkSprite").objectReferenceValue = ArtImports.Sprite("spark");
            so.FindProperty("ringSprite").objectReferenceValue = ArtImports.Sprite("ring");
            so.FindProperty("arrowSprite").objectReferenceValue = ArtImports.Sprite("arrow-up");
            so.FindProperty("lockOpenSprite").objectReferenceValue = ArtImports.Sprite("lock-open");
            so.FindProperty("coinIcon").objectReferenceValue = camera.transform.Find("HUD/Coin Icon").GetComponent<SpriteRenderer>();
            so.FindProperty("topBand").objectReferenceValue = camera.transform.Find("HUD/Top Band").GetComponent<SpriteRenderer>();
            so.FindProperty("bannerPill").objectReferenceValue = camera.transform.Find("HUD/Banner Pill").GetComponent<SpriteRenderer>();
            SetArray(so.FindProperty("locks"), locks);
            var cardsProperty = so.FindProperty("cards");
            cardsProperty.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++)
            {
                var element = cardsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("root").objectReferenceValue = cards[i].root;
                element.FindPropertyRelative("background").objectReferenceValue = cards[i].background;
                element.FindPropertyRelative("title").objectReferenceValue = cards[i].title;
                element.FindPropertyRelative("price").objectReferenceValue = cards[i].price;
            }
            so.FindProperty("enemyRoot").objectReferenceValue = roots[0];
            so.FindProperty("towerRoot").objectReferenceValue = roots[1];
            so.FindProperty("fxRoot").objectReferenceValue = roots[2];
            so.FindProperty("coinsText").objectReferenceValue = hud[0];
            so.FindProperty("phaseText").objectReferenceValue = hud[1];
            so.FindProperty("bannerText").objectReferenceValue = hud[2];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArray(SerializedProperty property, Object[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        /// <summary>Own empty lighting data: the built-in default ships a 0.5 MB reflection cubemap.</summary>
        static void BakeLighting()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(LightingPath);
            if (settings == null)
            {
                settings = new LightingSettings { bakedGI = false, realtimeGI = false };
                AssetDatabase.CreateAsset(settings, LightingPath);
            }
            Lightmapping.lightingSettings = settings;
            Lightmapping.Bake();
        }

        // ---------- Helpers ----------

        static GameObject Spawn(string assetPath, Transform parent)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath), parent);
            KitMaterials.Apply(instance, KitMaterials.ForAsset(assetPath));
            return instance;
        }

        static GameObject Rotated(GameObject go, float yaw)
        {
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            return go;
        }

        static float Top(GameObject go) =>
            go.GetComponentsInChildren<Renderer>().Select(r => r.bounds.max.y).Max() - go.transform.position.y;

        static void PlayIdle(GameObject pet)
        {
            var animation = pet.GetComponentInChildren<Animation>();
            var idle = animation != null ? animation.GetClip("idle") : null;
            if (idle == null) return;
            animation.clip = idle;
            animation.playAutomatically = true;
            animation.wrapMode = WrapMode.Loop;
        }
    }
}
