using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using JemparinganXR.Archery;
using JemparinganXR.Targets;
using JemparinganXR.GameFlow;
using JemparinganXR.UI;

namespace JemparinganXR.Editor
{
    /// <summary>
    /// Automated prototype scene generator for Jemparingan XR.
    /// Builds the complete playable scene with all components, materials, prefabs, and UI.
    /// Accessible via menu: Tools > Jemparingan XR > Build Prototype Scene
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        private const string RootDir = "Assets/JemparinganXR";
        private const string ScenesDir = RootDir + "/Scenes";
        private const string MaterialsDir = RootDir + "/Materials";
        private const string PrefabsDir = RootDir + "/Prefabs";
        private const string ScenePath = ScenesDir + "/Prototype.unity";
        private const string ArrowPrefabPath = PrefabsDir + "/ArrowPrefab.prefab";

        [MenuItem("Tools/Jemparingan XR/Build Prototype Scene")]
        public static void BuildPrototypeScene()
        {
            EnsureDirectories();
            MaterialPalette mats = CreateMaterials();
            Arrow arrowPrefab = CreateArrowPrefab(mats);
            BuildScene(mats, arrowPrefab);
            Debug.Log($"[Jemparingan XR] Prototype scene successfully built at: {ScenePath}");
        }

        public static void BuildSceneBatch()
        {
            BuildPrototypeScene();
        }

        private static void EnsureDirectories()
        {
            EnsureDir(RootDir);
            EnsureDir(ScenesDir);
            EnsureDir(MaterialsDir);
            EnsureDir(PrefabsDir);
            AssetDatabase.Refresh();
        }

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private class MaterialPalette
        {
            public Material ground;
            public Material mat;
            public Material wood;
            public Material bowWood;
            public Material bowString;
            public Material arrowShaft;
            public Material arrowHead;
            public Material arrowFeather;
            public Material molo;
            public Material awak;
            public Material klinthing;
            public Material buntut;
        }

        private static MaterialPalette CreateMaterials()
        {
            var p = new MaterialPalette();
            p.ground = GetOrCreateMaterial("Mat_Ground", new Color(0.24f, 0.40f, 0.20f), 0.2f);
            p.mat = GetOrCreateMaterial("Mat_ShootingMat", new Color(0.55f, 0.44f, 0.30f), 0.3f);
            p.wood = GetOrCreateMaterial("Mat_TargetWood", new Color(0.30f, 0.20f, 0.12f), 0.3f);
            p.bowWood = GetOrCreateMaterial("Mat_BowWood", new Color(0.45f, 0.25f, 0.12f), 0.6f);
            p.bowString = GetOrCreateMaterial("Mat_BowString", new Color(0.92f, 0.90f, 0.85f), 0.4f);
            p.arrowShaft = GetOrCreateMaterial("Mat_ArrowShaft", new Color(0.80f, 0.70f, 0.45f), 0.5f);
            p.arrowHead = GetOrCreateMaterial("Mat_ArrowHead", new Color(0.22f, 0.24f, 0.26f), 0.8f);
            p.arrowFeather = GetOrCreateMaterial("Mat_ArrowFeather", new Color(0.85f, 0.25f, 0.15f), 0.4f);
            p.molo = GetOrCreateMaterial("Mat_Molo", new Color(0.88f, 0.12f, 0.12f), 0.5f);
            p.awak = GetOrCreateMaterial("Mat_Awak", new Color(0.94f, 0.94f, 0.94f), 0.4f);
            p.klinthing = GetOrCreateMaterial("Mat_Klinthing", new Color(0.96f, 0.80f, 0.20f), 0.9f);
            p.buntut = GetOrCreateMaterial("Mat_Buntut", new Color(0.12f, 0.12f, 0.12f), 0.3f);
            return p;
        }

        private static Material GetOrCreateMaterial(string name, Color color, float smoothness)
        {
            string path = $"{MaterialsDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Diffuse");

                mat = new Material(shader);
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                }
                else
                {
                    mat.color = color;
                }
                if (mat.HasProperty("_Smoothness"))
                {
                    mat.SetFloat("_Smoothness", smoothness);
                }
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static Arrow CreateArrowPrefab(MaterialPalette mats)
        {
            GameObject arrowObj = new GameObject("ArrowPrefab");
            var col = arrowObj.AddComponent<CapsuleCollider>();
            col.radius = 0.02f;
            col.height = 0.78f;
            col.direction = 2; // Z-axis

            var rb = arrowObj.AddComponent<Rigidbody>();
            rb.mass = 0.04f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var arrowComp = arrowObj.AddComponent<Arrow>();

            // Shaft (Cylinder aligned along Z)
            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Shaft";
            shaft.transform.SetParent(arrowObj.transform, false);
            shaft.transform.localPosition = Vector3.zero;
            shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shaft.transform.localScale = new Vector3(0.012f, 0.35f, 0.012f);
            Object.DestroyImmediate(shaft.GetComponent<Collider>());
            shaft.GetComponent<MeshRenderer>().sharedMaterial = mats.arrowShaft;

            // Arrowhead (Tip)
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tip.name = "Tip";
            tip.transform.SetParent(arrowObj.transform, false);
            tip.transform.localPosition = new Vector3(0f, 0f, 0.37f);
            tip.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tip.transform.localScale = new Vector3(0.018f, 0.035f, 0.018f);
            Object.DestroyImmediate(tip.GetComponent<Collider>());
            tip.GetComponent<MeshRenderer>().sharedMaterial = mats.arrowHead;

            // Fletching (3 fins at back)
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f;
                GameObject fin = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fin.name = $"Fin_{i}";
                fin.transform.SetParent(arrowObj.transform, false);
                fin.transform.localPosition = new Vector3(0f, 0f, -0.28f);
                fin.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                fin.transform.localScale = new Vector3(0.003f, 0.05f, 0.12f);
                Object.DestroyImmediate(fin.GetComponent<Collider>());
                fin.GetComponent<MeshRenderer>().sharedMaterial = mats.arrowFeather;
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(arrowObj, ArrowPrefabPath);
            Object.DestroyImmediate(arrowObj);
            return prefab.GetComponent<Arrow>();
        }

        private static void BuildScene(MaterialPalette mats, Arrow arrowPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting
            GameObject lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.90f);
            light.intensity = 1.25f;
            lightGo.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            // Environment: Ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, 0f, 15f);
            ground.transform.localScale = new Vector3(6f, 1f, 6f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = mats.ground;

            // Shooting mat (Plasa / Klosa for seated archer)
            GameObject matGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            matGo.name = "ShootingMat_Sila";
            matGo.transform.position = new Vector3(0f, 0.02f, 0f);
            matGo.transform.localScale = new Vector3(1.6f, 0.04f, 1.6f);
            matGo.GetComponent<MeshRenderer>().sharedMaterial = mats.mat;

            // Archer Station
            GameObject archerStation = new GameObject("ArcherStation");
            archerStation.transform.position = Vector3.zero;
            var inputComp = archerStation.AddComponent<MouseKeyboardArcheryInput>();

            // Archer Camera (Seated eye height ~1.0m)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(archerStation.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<AudioListener>();
            var camController = camGo.AddComponent<ArcherCameraController>();

            // Bow (Gandhewa) mounted to camera
            BowController bowComp = CreateBowHierarchy(camGo.transform, mats, arrowPrefab);

            // Target (Bandulan)
            BandulanTarget targetComp = CreateBandulanTarget(mats, 15f);

            // GameFlow Manager
            GameObject gameFlowGo = new GameObject("GameFlowManager");
            var gameFlowComp = gameFlowGo.AddComponent<GameFlowManager>();
            var targetField = typeof(GameFlowManager).GetField("target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (targetField != null) targetField.SetValue(gameFlowComp, targetComp);

            // Archery Gameplay Manager
            var gameplayMgr = archerStation.AddComponent<ArcheryGameplayManager>();
            SetPrivateField(gameplayMgr, "cameraController", camController);
            SetPrivateField(gameplayMgr, "bowController", bowComp);
            SetPrivateField(gameplayMgr, "gameFlowManager", gameFlowComp);
            SetPrivateField(gameplayMgr, "aimTransform", camGo.transform);

            // UI Canvas & HUD
            CreateUI(gameFlowComp, bowComp);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static BowController CreateBowHierarchy(Transform cameraTransform, MaterialPalette mats, Arrow arrowPrefab)
        {
            GameObject bowRoot = new GameObject("GandhewaBow");
            bowRoot.transform.SetParent(cameraTransform, false);
            bowRoot.transform.localPosition = new Vector3(0.18f, -0.16f, 0.45f);
            bowRoot.transform.localRotation = Quaternion.Euler(6f, -10f, 14f);

            var bowComp = bowRoot.AddComponent<BowController>();

            // Handle / Riser
            GameObject riser = GameObject.CreatePrimitive(PrimitiveType.Cube);
            riser.name = "Riser";
            riser.transform.SetParent(bowRoot.transform, false);
            riser.transform.localScale = new Vector3(0.035f, 0.16f, 0.045f);
            Object.DestroyImmediate(riser.GetComponent<Collider>());
            riser.GetComponent<MeshRenderer>().sharedMaterial = mats.bowWood;

            // Upper Limb
            GameObject upperLimb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            upperLimb.name = "UpperLimb";
            upperLimb.transform.SetParent(bowRoot.transform, false);
            upperLimb.transform.localPosition = new Vector3(0f, 0.28f, 0.04f);
            upperLimb.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            upperLimb.transform.localScale = new Vector3(0.025f, 0.42f, 0.03f);
            Object.DestroyImmediate(upperLimb.GetComponent<Collider>());
            upperLimb.GetComponent<MeshRenderer>().sharedMaterial = mats.bowWood;

            // Lower Limb
            GameObject lowerLimb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lowerLimb.name = "LowerLimb";
            lowerLimb.transform.SetParent(bowRoot.transform, false);
            lowerLimb.transform.localPosition = new Vector3(0f, -0.28f, 0.04f);
            lowerLimb.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);
            lowerLimb.transform.localScale = new Vector3(0.025f, 0.42f, 0.03f);
            Object.DestroyImmediate(lowerLimb.GetComponent<Collider>());
            lowerLimb.GetComponent<MeshRenderer>().sharedMaterial = mats.bowWood;

            // String Tips & Nock
            GameObject upperTip = new GameObject("UpperStringTip");
            upperTip.transform.SetParent(bowRoot.transform, false);
            upperTip.transform.localPosition = new Vector3(0f, 0.48f, -0.02f);

            GameObject lowerTip = new GameObject("LowerStringTip");
            lowerTip.transform.SetParent(bowRoot.transform, false);
            lowerTip.transform.localPosition = new Vector3(0f, -0.48f, -0.02f);

            GameObject nock = new GameObject("StringNockPoint");
            nock.transform.SetParent(bowRoot.transform, false);
            nock.transform.localPosition = new Vector3(0f, 0f, -0.02f);

            GameObject spawnPoint = new GameObject("ArrowSpawnPoint");
            spawnPoint.transform.SetParent(bowRoot.transform, false);
            spawnPoint.transform.localPosition = new Vector3(0f, 0.02f, 0.10f);

            // Bowstring LineRenderer
            var line = bowRoot.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.startWidth = 0.005f;
            line.endWidth = 0.005f;
            line.positionCount = 3;
            line.sharedMaterial = mats.bowString;

            // Loaded Arrow Visual on Bow
            GameObject loadedArrow = new GameObject("LoadedArrowVisual");
            loadedArrow.transform.SetParent(nock.transform, false);
            loadedArrow.transform.localPosition = Vector3.zero;

            GameObject lShaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lShaft.name = "LoadedShaft";
            lShaft.transform.SetParent(loadedArrow.transform, false);
            lShaft.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            lShaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lShaft.transform.localScale = new Vector3(0.012f, 0.35f, 0.012f);
            Object.DestroyImmediate(lShaft.GetComponent<Collider>());
            lShaft.GetComponent<MeshRenderer>().sharedMaterial = mats.arrowShaft;

            SetPrivateField(bowComp, "arrowPrefab", arrowPrefab);
            SetPrivateField(bowComp, "arrowSpawnPoint", spawnPoint.transform);
            SetPrivateField(bowComp, "upperStringTip", upperTip.transform);
            SetPrivateField(bowComp, "lowerStringTip", lowerTip.transform);
            SetPrivateField(bowComp, "stringNockPoint", nock.transform);
            SetPrivateField(bowComp, "bowstringRenderer", line);
            SetPrivateField(bowComp, "loadedArrowVisual", loadedArrow);

            bowComp.UpdateBowstringVisual();
            return bowComp;
        }

        private static BandulanTarget CreateBandulanTarget(MaterialPalette mats, float initialDistance)
        {
            GameObject targetStation = new GameObject("BandulanTargetStation");
            targetStation.transform.position = new Vector3(0f, 0f, initialDistance);

            var targetComp = targetStation.AddComponent<BandulanTarget>();

            // Wooden Gallows Frame
            GameObject frame = new GameObject("Frame");
            frame.transform.SetParent(targetStation.transform, false);

            // Left Post
            GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postL.name = "Post_Left";
            postL.transform.SetParent(frame.transform, false);
            postL.transform.localPosition = new Vector3(-0.85f, 1.3f, 0f);
            postL.transform.localScale = new Vector3(0.08f, 2.6f, 0.08f);
            postL.GetComponent<MeshRenderer>().sharedMaterial = mats.wood;

            // Right Post
            GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postR.name = "Post_Right";
            postR.transform.SetParent(frame.transform, false);
            postR.transform.localPosition = new Vector3(0.85f, 1.3f, 0f);
            postR.transform.localScale = new Vector3(0.08f, 2.6f, 0.08f);
            postR.GetComponent<MeshRenderer>().sharedMaterial = mats.wood;

            // Top Crossbar
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Crossbar";
            beam.transform.SetParent(frame.transform, false);
            beam.transform.localPosition = new Vector3(0f, 2.55f, 0f);
            beam.transform.localScale = new Vector3(1.9f, 0.08f, 0.08f);
            beam.GetComponent<MeshRenderer>().sharedMaterial = mats.wood;

            // Suspension Pivot (Pivot for pendulum swing)
            GameObject pivot = new GameObject("SuspensionPivot");
            pivot.transform.SetParent(targetStation.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 2.5f, 0f);

            // Rope hanging down (length ~1.2m)
            GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rope.name = "Rope";
            rope.transform.SetParent(pivot.transform, false);
            rope.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            rope.transform.localScale = new Vector3(0.01f, 0.6f, 0.01f);
            Object.DestroyImmediate(rope.GetComponent<Collider>());
            rope.GetComponent<MeshRenderer>().sharedMaterial = mats.bowString;

            // Bandulan Cylinder Assembly (Center at Y = ~1.0m from ground, matches seated archer!)
            GameObject bandulan = new GameObject("BandulanCylinder");
            bandulan.transform.SetParent(pivot.transform, false);
            bandulan.transform.localPosition = new Vector3(0f, -1.35f, 0f);

            // 1. Klinthingan (Brass Bell at top - 5 Points)
            GameObject klinthing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            klinthing.name = "Zone_Klinthing";
            klinthing.transform.SetParent(bandulan.transform, false);
            klinthing.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            klinthing.transform.localScale = new Vector3(0.07f, 0.04f, 0.07f);
            klinthing.GetComponent<MeshRenderer>().sharedMaterial = mats.klinthing;
            var kZone = klinthing.AddComponent<TargetHitZone>();
            SetPrivateField(kZone, "zoneName", "Klinthing (Genta)");
            SetPrivateField(kZone, "pointValue", 5);
            SetPrivateField(kZone, "hitHighlightColor", Color.white);

            // 2. Molo (Head / Red Section - 3 Points)
            GameObject molo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            molo.name = "Zone_Molo";
            molo.transform.SetParent(bandulan.transform, false);
            molo.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            molo.transform.localScale = new Vector3(0.065f, 0.08f, 0.065f);
            molo.GetComponent<MeshRenderer>().sharedMaterial = mats.molo;
            var mZone = molo.AddComponent<TargetHitZone>();
            SetPrivateField(mZone, "zoneName", "Molo (Kepala)");
            SetPrivateField(mZone, "pointValue", 3);
            SetPrivateField(mZone, "hitHighlightColor", Color.yellow);

            // 3. Awak (Body / White Section - 1 Point)
            GameObject awak = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            awak.name = "Zone_Awak";
            awak.transform.SetParent(bandulan.transform, false);
            awak.transform.localPosition = new Vector3(0f, -0.07f, 0f);
            awak.transform.localScale = new Vector3(0.065f, 0.22f, 0.065f);
            awak.GetComponent<MeshRenderer>().sharedMaterial = mats.awak;
            var aZone = awak.AddComponent<TargetHitZone>();
            SetPrivateField(aZone, "zoneName", "Awak (Badan)");
            SetPrivateField(aZone, "pointValue", 1);
            SetPrivateField(aZone, "hitHighlightColor", Color.red);

            // 4. Buntut (Bottom Weight - 0 Point)
            GameObject buntut = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            buntut.name = "Zone_Buntut";
            buntut.transform.SetParent(bandulan.transform, false);
            buntut.transform.localPosition = new Vector3(0f, -0.22f, 0f);
            buntut.transform.localScale = new Vector3(0.045f, 0.05f, 0.045f);
            buntut.GetComponent<MeshRenderer>().sharedMaterial = mats.buntut;
            var bZone = buntut.AddComponent<TargetHitZone>();
            SetPrivateField(bZone, "zoneName", "Buntut (Bawah)");
            SetPrivateField(bZone, "pointValue", 0);
            SetPrivateField(bZone, "hitHighlightColor", Color.gray);

            SetPrivateField(targetComp, "standRoot", targetStation.transform);
            SetPrivateField(targetComp, "suspensionPivot", pivot.transform);
            SetPrivateField(targetComp, "bandulanCylinder", bandulan.transform);

            return targetComp;
        }

        private static void CreateUI(GameFlowManager gameFlow, BowController bow)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Canvas
            GameObject canvasGo = new GameObject("ArcheryCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            // EventSystem
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif

            // ArcheryHUD component
            var hud = canvasGo.AddComponent<ArcheryHUD>();
            SetPrivateField(hud, "gameFlowManager", gameFlow);
            SetPrivateField(hud, "bowController", bow);

            // HUD Top-Left Panel (Round & Arrow Info)
            GameObject leftPanel = CreateUIPanel(canvasGo.transform, "InfoPanel_Left", new Vector2(20f, -20f), new Vector2(380f, 150f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            Text rambahanText = CreateUIText(leftPanel.transform, "RambahanText", "Rambahan: 1 / 5", font, 24, TextAnchor.MiddleLeft, new Vector2(15f, -15f), new Vector2(350f, 35f), Color.white);
            Text arrowText = CreateUIText(leftPanel.transform, "ArrowText", "Panah: 1 / 4 (Total: 0 / 20)", font, 20, TextAnchor.MiddleLeft, new Vector2(15f, -50f), new Vector2(350f, 30f), new Color(0.9f, 0.9f, 0.9f));
            Text objectiveText = CreateUIText(leftPanel.transform, "ObjectiveText", "Target: 4 Poin (15m)", font, 18, TextAnchor.MiddleLeft, new Vector2(15f, -85f), new Vector2(350f, 30f), new Color(0.95f, 0.8f, 0.3f));

            // HUD Top-Right Panel (Scores & Shot Results)
            GameObject rightPanel = CreateUIPanel(canvasGo.transform, "ScorePanel_Right", new Vector2(-20f, -20f), new Vector2(380f, 150f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            Text roundScoreText = CreateUIText(rightPanel.transform, "RoundScoreText", "Skor Babak: 0", font, 22, TextAnchor.MiddleRight, new Vector2(-15f, -15f), new Vector2(350f, 35f), Color.white);
            Text totalScoreText = CreateUIText(rightPanel.transform, "TotalScoreText", "Total Skor: 0", font, 26, TextAnchor.MiddleRight, new Vector2(-15f, -50f), new Vector2(350f, 35f), new Color(1f, 0.85f, 0.3f));
            Text lastShotText = CreateUIText(rightPanel.transform, "LastShotText", "Hasil: -", font, 19, TextAnchor.MiddleRight, new Vector2(-15f, -88f), new Vector2(350f, 30f), new Color(0.6f, 0.9f, 1f));

            // Crosshair (Center Dot)
            GameObject crosshair = new GameObject("Crosshair");
            crosshair.transform.SetParent(canvasGo.transform, false);
            var chRect = crosshair.AddComponent<RectTransform>();
            chRect.anchorMin = chRect.anchorMax = new Vector2(0.5f, 0.5f);
            chRect.sizeDelta = new Vector2(12f, 12f);
            var chImg = crosshair.AddComponent<Image>();
            chImg.color = new Color(1f, 1f, 1f, 0.75f);

            // Draw Tension Meter (Bottom Center)
            GameObject meterBg = CreateUIPanel(canvasGo.transform, "DrawMeterBg", new Vector2(0f, 60f), new Vector2(340f, 26f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            GameObject meterFill = new GameObject("DrawMeterFill");
            meterFill.transform.SetParent(meterBg.transform, false);
            var fillRect = meterFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = meterFill.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 0f;
            fillImg.color = new Color(0.2f, 0.8f, 0.2f);
            CreateUIText(meterBg.transform, "DrawLabel", "[LMB] Tarik Gandhewa (Tahan & Lepas)", font, 14, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(340f, 26f), Color.white);

            // Modal 1: Round Summary Panel
            GameObject roundPanel = CreateUIPanel(canvasGo.transform, "RoundSummaryPanel", Vector2.zero, new Vector2(560f, 320f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.08f, 0.08f, 0.1f, 0.92f));
            Text rTitle = CreateUIText(roundPanel.transform, "RTitle", "Rambahan 1 Selesai", font, 30, TextAnchor.UpperCenter, new Vector2(0f, -25f), new Vector2(500f, 40f), Color.white);
            Text rScore = CreateUIText(roundPanel.transform, "RScore", "Skor Babak: 0  |  Target: 4", font, 22, TextAnchor.MiddleCenter, new Vector2(0f, -90f), new Vector2(500f, 35f), new Color(1f, 0.85f, 0.3f));
            Text rStatus = CreateUIText(roundPanel.transform, "RStatus", "Objektif Tercapai!", font, 20, TextAnchor.MiddleCenter, new Vector2(0f, -145f), new Vector2(500f, 35f), Color.green);
            Button rBtn = CreateUIButton(roundPanel.transform, "NextRoundButton", "Lanjut ke Rambahan Berikutnya [Spasi]", font, new Vector2(0f, -225f), new Vector2(380f, 50f));

            // Modal 2: Game Over Panel
            GameObject overPanel = CreateUIPanel(canvasGo.transform, "GameOverPanel", Vector2.zero, new Vector2(620f, 420f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.08f, 0.08f, 0.1f, 0.95f));
            CreateUIText(overPanel.transform, "OverTitle", "Gladhen Jemparingan Selesai", font, 32, TextAnchor.UpperCenter, new Vector2(0f, -25f), new Vector2(550f, 45f), new Color(1f, 0.85f, 0.3f));
            Text oTotal = CreateUIText(overPanel.transform, "OverTotal", "Total Skor Akhir: 0 Poin", font, 24, TextAnchor.MiddleCenter, new Vector2(0f, -85f), new Vector2(550f, 35f), Color.white);
            Text oBreak = CreateUIText(overPanel.transform, "OverBreakdown", "Ringkasan 5 Rambahan...", font, 18, TextAnchor.UpperLeft, new Vector2(0f, -135f), new Vector2(460f, 150f), new Color(0.85f, 0.85f, 0.85f));
            Button oBtn = CreateUIButton(overPanel.transform, "PlayAgainButton", "Main Lagi [R]", font, new Vector2(0f, -345f), new Vector2(300f, 50f));

            roundPanel.SetActive(false);
            overPanel.SetActive(false);

            SetPrivateField(hud, "rambahanText", rambahanText);
            SetPrivateField(hud, "arrowText", arrowText);
            SetPrivateField(hud, "roundScoreText", roundScoreText);
            SetPrivateField(hud, "totalScoreText", totalScoreText);
            SetPrivateField(hud, "lastShotText", lastShotText);
            SetPrivateField(hud, "objectiveText", objectiveText);
            SetPrivateField(hud, "drawGaugeFill", fillImg);
            SetPrivateField(hud, "crosshair", crosshair);
            SetPrivateField(hud, "roundSummaryPanel", roundPanel);
            SetPrivateField(hud, "roundSummaryTitle", rTitle);
            SetPrivateField(hud, "roundSummaryScoreText", rScore);
            SetPrivateField(hud, "roundSummaryStatusText", rStatus);
            SetPrivateField(hud, "nextRoundButton", rBtn);
            SetPrivateField(hud, "gameOverPanel", overPanel);
            SetPrivateField(hud, "gameOverTotalScoreText", oTotal);
            SetPrivateField(hud, "gameOverBreakdownText", oBreak);
            SetPrivateField(hud, "playAgainButton", oBtn);
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Color? bgColor = null)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = panel.AddComponent<Image>();
            img.color = bgColor ?? new Color(0.1f, 0.1f, 0.14f, 0.75f);
            return panel;
        }

        private static Text CreateUIText(Transform parent, string name, string text, Font font, int fontSize, TextAnchor alignment, Vector2 pos, Vector2 size, Color color)
        {
            GameObject textGo = new GameObject(name);
            textGo.transform.SetParent(parent, false);
            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var t = textGo.AddComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.color = color;
            return t;
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Font font, Vector2 pos, Vector2 size)
        {
            GameObject btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.22f, 0.55f, 0.35f, 0.95f);

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.28f, 0.68f, 0.42f, 1f);
            colors.pressedColor = new Color(0.18f, 0.45f, 0.28f, 1f);
            btn.colors = colors;

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var tRect = textGo.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            var t = textGo.AddComponent<Text>();
            t.text = label;
            t.font = font;
            t.fontSize = 18;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;

            return btn;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
