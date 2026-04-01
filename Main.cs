using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Namespace AssetExporter muss zu deiner AssemblyInfo passen
[assembly: MelonInfo(typeof(AssetExporter.Main), "Asset Exporter", "1.0.0", "DeinName")]
[assembly: MelonGame(null, null)]

namespace AssetExporter
{
    public class Main : MelonMod
    {
        private string exportPath = string.Empty;

        public override void OnInitializeMelon()
        {
            // Erstellt den Ordner im Spielverzeichnis/Mods/ExportedAssets
            exportPath = Path.Combine(MelonEnvironment.ModsDirectory, "ExportedAssets");
            if (!Directory.Exists(exportPath)) Directory.CreateDirectory(exportPath);

            MelonLogger.Msg("Asset Exporter geladen. Drücke F8 im Spiel (während ein Save geladen ist).");
        }

        public override void OnUpdate()
        {
            if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
            {
                ExportAllResources();
            }

            if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
            {
                LogUiPathUnderCursor();
            }
        }

        private void ExportAllResources()
        {
            string currentGamePath = Path.Combine(exportPath, "CurrentGame");
            string betaNotUsedPath = Path.Combine(currentGamePath, "Beta_NotUsed_Unity6");
            Directory.CreateDirectory(currentGamePath);
            Directory.CreateDirectory(betaNotUsedPath);

            File.WriteAllText(
                Path.Combine(currentGamePath, "README_NOT_USED.txt"),
                "Dieser Ordner enthält verwendete Assets aus dem aktuellen Spielstand (aktiv + inaktiv).\n" +
                "Nicht verwendete, aber geladene Assets wurden zu Beta-Zwecken nach 'Beta_NotUsed_Unity6' exportiert."
            );

            MelonLogger.Msg("Starte Export: verwendete Assets (aktiv + inaktiv) aus allen geladenen Szenen...");

            HashSet<int> usedMeshIds = new HashSet<int>();
            HashSet<int> usedTextureIds = new HashSet<int>();
            HashSet<string> exportedCurrentGame = new HashSet<string>();

            foreach (GameObject obj in EnumerateAllSceneObjects(includeInactive: true))
            {
                try
                {
                    MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        usedMeshIds.Add(meshFilter.sharedMesh.GetInstanceID());
                        if (TryRegister(exportedCurrentGame, $"mesh:{meshFilter.sharedMesh.name}"))
                            SaveMesh(meshFilter.sharedMesh, currentGamePath);
                    }

                    SkinnedMeshRenderer skinnedMeshRenderer = obj.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
                    {
                        usedMeshIds.Add(skinnedMeshRenderer.sharedMesh.GetInstanceID());
                        if (TryRegister(exportedCurrentGame, $"mesh:{skinnedMeshRenderer.sharedMesh.name}"))
                            SaveMesh(skinnedMeshRenderer.sharedMesh, currentGamePath);
                    }

                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material[] materials = renderer.sharedMaterials;
                        foreach (Material material in materials)
                        {
                            if (material == null) continue;

                            string[] texturePropertyNames = material.GetTexturePropertyNames();
                            foreach (string propertyName in texturePropertyNames)
                            {
                                Texture texture = material.GetTexture(propertyName);
                                if (texture is Texture2D tex2D)
                                {
                                    usedTextureIds.Add(tex2D.GetInstanceID());
                                    if (TryRegister(exportedCurrentGame, $"tex:{tex2D.name}"))
                                        SaveTexture(tex2D, currentGamePath);
                                }
                            }
                        }
                    }

                    Component uiImage = obj.GetComponent("Image");
                    if (uiImage != null)
                    {
                        PropertyInfo spriteProperty = uiImage.GetType().GetProperty("sprite");
                        Sprite sprite = spriteProperty?.GetValue(uiImage) as Sprite;
                        if (sprite != null && sprite.texture != null)
                        {
                            usedTextureIds.Add(sprite.texture.GetInstanceID());
                            if (TryRegister(exportedCurrentGame, $"tex:{sprite.texture.name}"))
                                SaveTexture(sprite.texture, currentGamePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning($"Export-Fehler bei Objekt '{obj.name}': {ex.Message}");
                }
            }

            MelonLogger.Msg("Starte Beta-Export: nicht verwendete, aber geladene Assets...");

            HashSet<string> exportedBeta = new HashSet<string>();

            foreach (Mesh mesh in Resources.FindObjectsOfTypeAll<Mesh>())
            {
                if (mesh == null) continue;
                if (usedMeshIds.Contains(mesh.GetInstanceID())) continue;
                if (!TryRegister(exportedBeta, $"mesh:{mesh.name}")) continue;
                SaveMesh(mesh, betaNotUsedPath);
            }

            foreach (Texture2D tex in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                if (tex == null) continue;
                if (usedTextureIds.Contains(tex.GetInstanceID())) continue;
                if (!TryRegister(exportedBeta, $"tex:{tex.name}")) continue;
                SaveTexture(tex, betaNotUsedPath);
            }

            MelonLogger.Msg($"Export abgeschlossen! Verbaute Assets: {currentGamePath} | Beta (nicht verwendet): {betaNotUsedPath}");
        }

        private IEnumerable<GameObject> EnumerateAllSceneObjects(bool includeInactive)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;

                GameObject[] roots = scene.GetRootGameObjects();
                foreach (GameObject root in roots)
                {
                    if (root == null) continue;

                    Transform[] allTransforms = root.GetComponentsInChildren<Transform>(includeInactive);
                    foreach (Transform t in allTransforms)
                    {
                        if (t != null && t.gameObject != null)
                            yield return t.gameObject;
                    }
                }
            }
        }

        private void LogUiPathUnderCursor()
        {
            if (Mouse.current == null)
            {
                MelonLogger.Warning("Keine Maus verfügbar.");
                return;
            }

            Type eventSystemType = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
            Type pointerEventDataType = Type.GetType("UnityEngine.EventSystems.PointerEventData, UnityEngine.UI");
            Type raycastResultType = Type.GetType("UnityEngine.EventSystems.RaycastResult, UnityEngine.UI");

            if (eventSystemType == null || pointerEventDataType == null || raycastResultType == null)
            {
                MelonLogger.Warning("UI EventSystem-Typen konnten nicht aufgelöst werden.");
                return;
            }

            object currentEventSystem = eventSystemType.GetProperty("current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (currentEventSystem == null)
            {
                MelonLogger.Warning("Kein aktives EventSystem gefunden.");
                return;
            }

            object pointerEventData = Activator.CreateInstance(pointerEventDataType, currentEventSystem);
            pointerEventDataType.GetProperty("position")?.SetValue(pointerEventData, Mouse.current.position.ReadValue());

            Type listType = typeof(List<>).MakeGenericType(raycastResultType);
            object results = Activator.CreateInstance(listType);
            eventSystemType.GetMethod("RaycastAll")?.Invoke(currentEventSystem, new[] { pointerEventData, results });

            int resultCount = (int)(listType.GetProperty("Count")?.GetValue(results) ?? 0);

            if (resultCount == 0)
            {
                MelonLogger.Msg("Kein UI-Element unter dem Cursor gefunden.");
                return;
            }

            foreach (object result in (System.Collections.IEnumerable)results)
            {
                GameObject gameObject = raycastResultType.GetProperty("gameObject")?.GetValue(result) as GameObject;
                if (gameObject == null) continue;

                string path = gameObject.name;
                Transform parent = gameObject.transform.parent;

                while (parent != null)
                {
                    path = parent.name + "/" + path;
                    parent = parent.parent;
                }

                MelonLogger.Msg("UI-Pfad gefunden: " + path);
            }
        }

        private static bool TryRegister(HashSet<string> exportedNames, string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return false;
            if (rawName.Contains("unity", StringComparison.OrdinalIgnoreCase)) return false;
            return exportedNames.Add(rawName);
        }

        private void SaveTexture(Texture2D tex, string targetDirectory)
        {
            if (tex == null || string.IsNullOrEmpty(tex.name) || tex.name.Contains("unity")) return;
            if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

            // RenderTexture Trick um Read/Write-Sperre zu umgehen
            RenderTexture tmp = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(tex, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;

            Texture2D readableTex = new Texture2D(tex.width, tex.height);
            readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableTex.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            byte[] bytes = ImageConversion.EncodeToPNG(readableTex);
            string safeName = string.Join("_", tex.name.Split(Path.GetInvalidFileNameChars()));
            string filePath = EnsureUniquePath(targetDirectory, safeName, ".png");
            File.WriteAllBytes(filePath, bytes);

            // Objekt zerstören um Speicher zu sparen während des Exports
            UnityEngine.Object.Destroy(readableTex);
        }

        private void SaveMesh(Mesh mesh, string targetDirectory)
        {
            if (mesh == null || string.IsNullOrEmpty(mesh.name) || mesh.name.Contains("unity")) return;
            if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

            string safeName = string.Join("_", mesh.name.Split(Path.GetInvalidFileNameChars()));
            string filePath = EnsureUniquePath(targetDirectory, safeName, ".obj");

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("g ").Append(safeName).Append("\n");

            // Nutze die expliziten Unity-Typen um Konflikte mit System.Numerics zu vermeiden
            foreach (UnityEngine.Vector3 v in mesh.vertices)
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z).Replace(",", "."));

            sb.Append("\n");

            foreach (UnityEngine.Vector3 v in mesh.normals)
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z).Replace(",", "."));

            sb.Append("\n");

            foreach (UnityEngine.Vector2 v in mesh.uv)
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y).Replace(",", "."));

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] triangles = mesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    // OBJ Format Indizes starten bei 1
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                        triangles[j] + 1, triangles[j + 1] + 1, triangles[j + 2] + 1));
                }
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        private static string EnsureUniquePath(string directory, string baseName, string extension)
        {
            string filePath = Path.Combine(directory, baseName + extension);
            int i = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(directory, $"{baseName}_{i}{extension}");
                i++;
            }

            return filePath;
        }
    }
}