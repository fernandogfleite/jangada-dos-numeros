using System;
using JangadaDosNumeros.Core;
using JangadaDosNumeros.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace JangadaDosNumeros.Editor
{
    [InitializeOnLoad]
    public static class VerticalSliceBuilder
    {
        private const string ScenePath = "Assets/JangadaDosNumeros/Scenes/VerticalSlice.unity";
        private const string PrefabFolder = "Assets/JangadaDosNumeros/Generated/Prefabs";
        private const string SettingsFolder = "Assets/JangadaDosNumeros/Generated/Settings";
        private const string PipelinePath = SettingsFolder + "/JangadaURP.asset";

        static VerticalSliceBuilder()
        {
            EditorApplication.delayCall += OpenMainSceneOnFirstOpen;
        }

        [MenuItem("Tools/Jangada dos Números/Gerar vertical slice")]
        public static void Generate()
        {
            if (!CanReplaceCurrentScene())
            {
                return;
            }

            GenerateAndOpenMainScene();
        }

        [MenuItem("Tools/Jangada dos Números/Abrir cena principal")]
        public static void OpenMainScene()
        {
            if (!CanReplaceCurrentScene())
            {
                return;
            }

            if (!System.IO.File.Exists(ScenePath))
            {
                GenerateAndOpenMainScene();
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static void GenerateAndOpenMainScene()
        {
            EnsureFolders();
            ConfigurePipeline();
            GenerateCargoPrefabs();
            GenerateScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Jangada dos Números: cena e prefabs gerados em Assets/JangadaDosNumeros.");
        }

        private static void OpenMainSceneOnFirstOpen()
        {
            if (Application.isBatchMode || !IsUntouchedDefaultScene(EditorSceneManager.GetActiveScene()))
            {
                return;
            }

            if (!System.IO.File.Exists(ScenePath))
            {
                GenerateAndOpenMainScene();
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static bool CanReplaceCurrentScene()
        {
            return Application.isBatchMode || EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        private static bool IsUntouchedDefaultScene(Scene scene)
        {
            if (!scene.IsValid()
                || !scene.isLoaded
                || scene.isDirty
                || !string.IsNullOrEmpty(scene.path)
                || !string.Equals(scene.name, "Untitled", StringComparison.Ordinal))
            {
                return false;
            }

            var roots = scene.GetRootGameObjects();
            if (roots.Length != 2)
            {
                return false;
            }

            GameObject mainCamera = null;
            GameObject directionalLight = null;

            foreach (var root in roots)
            {
                if (root.name == "Main Camera")
                {
                    mainCamera = root;
                }
                else if (root.name == "Directional Light")
                {
                    directionalLight = root;
                }
            }

            var light = directionalLight != null ? directionalLight.GetComponent<Light>() : null;
            return mainCamera != null
                && mainCamera.transform.childCount == 0
                && mainCamera.GetComponent<Camera>() != null
                && mainCamera.GetComponent<AudioListener>() != null
                && directionalLight != null
                && directionalLight.transform.childCount == 0
                && light != null
                && light.type == LightType.Directional;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "JangadaDosNumeros");
            EnsureFolder("Assets/JangadaDosNumeros", "Scenes");
            EnsureFolder("Assets/JangadaDosNumeros", "Generated");
            EnsureFolder("Assets/JangadaDosNumeros/Generated", "Prefabs");
            EnsureFolder("Assets/JangadaDosNumeros/Generated", "Settings");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void ConfigurePipeline()
        {
            PlayerSettings.companyName = "NEES";
            PlayerSettings.productName = "Jangada dos Numeros";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
                var renderer = pipeline.LoadBuiltinRendererData(RendererType.UniversalRenderer);
                renderer.name = "JangadaRenderer";
                if (!AssetDatabase.Contains(renderer))
                {
                    AssetDatabase.AddObjectToAsset(renderer, pipeline);
                }
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        private static void GenerateCargoPrefabs()
        {
            foreach (CargoType cargo in Enum.GetValues(typeof(CargoType)))
            {
                var temporary = BlockoutFactory.CreateCargoObject(cargo, "Carga_" + cargo, null, true);
                PrefabUtility.SaveAsPrefabAsset(temporary, PrefabFolder + "/Carga_" + cargo + ".prefab");
                UnityEngine.Object.DestroyImmediate(temporary);
            }
        }

        private static void GenerateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("JangadaDosNumeros");
            var controller = root.AddComponent<VerticalSliceController>();
            controller.RebuildBlockout();
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            SceneManager.SetActiveScene(scene);
        }
    }
}

