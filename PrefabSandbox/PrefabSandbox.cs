#if UNITY_EDITOR
﻿using DT;
using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
﻿using UnityEngine;
﻿using UnityEngine.SceneManagement;

namespace DT.Prefab {
	[InitializeOnLoad]
	public class PrefabSandbox {
		private const string kSandboxSceneName = "PrefabSandbox";
		private const string kSandboxSetupPrefabName = "PrefabSandboxSetupPrefab";

		private static readonly string kSandboxScenePath = PrefabSandbox.FindSandboxPath();

    [Serializable]
		public class PrefabSandboxData {
			public string prefabGuid;
			public string prefabPath;
			public GameObject prefabAsset;
			public GameObject prefabInstance;

      public string oldScenePath;
		}

		private static PrefabSandboxData _data;
    private static Scene _sandboxScene;

		private static GameObject _sandboxSetupPrefab;

		static PrefabSandbox() {
			string sandboxSetupPrefabPath = PrefabSandbox.FindSandboxSetupPrefabPath();
			PrefabSandbox._sandboxSetupPrefab = AssetDatabase.LoadAssetAtPath(sandboxSetupPrefabPath, typeof(GameObject)) as GameObject;

			EditorApplicationUtil.OnSceneGUIDelegate += PrefabSandbox.OnSceneGUI;
		}


		// PRAGMA MARK - Public Interface
		public static bool OpenPrefab(string guid) {
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;

			return PrefabSandbox.OpenPrefab(prefab);
		}

		public static bool OpenPrefab(GameObject prefab) {
			string assetPath = AssetDatabase.GetAssetPath(prefab);
			string guid = AssetDatabase.AssetPathToGUID(assetPath);

      Scene oldScene = EditorSceneManager.GetActiveScene();

			bool alreadyEditing = (PrefabSandbox._data != null && PrefabSandbox._data.prefabGuid == guid);

			if (prefab != null && assetPath != null && PathUtil.IsPrefab(assetPath) && !alreadyEditing) {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
          return false;
        }

				PrefabSandbox._data = new PrefabSandboxData();
				PrefabSandbox._data.prefabGuid = guid;
				PrefabSandbox._data.prefabPath = assetPath;
				PrefabSandbox._data.prefabAsset = prefab;
        PrefabSandbox._data.oldScenePath = oldScene.path;

        PrefabSandbox.SavePrefabData();
				PrefabSandbox.SetupSandbox();
				return true;
			} else {
				return false;
			}
		}

		public static void SavePrefabAsset() {
			if (PrefabSandbox._data != null && PrefabSandbox._data.prefabInstance != null && PrefabSandbox._data.prefabAsset != null) {
				UnityEditor.AnimationMode.StopAnimationMode();

				PrefabUtility.ReplacePrefab(PrefabSandbox._data.prefabInstance, PrefabSandbox._data.prefabAsset, ReplacePrefabOptions.Default);
				SceneView.RepaintAll();
			}
		}

		public static bool IsEditing() {
			return PrefabSandbox._data != null;
		}


		// PRAGMA MARK - Static Internal
    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsReloaded() {
      PrefabSandbox.ReloadPrefabData();
    }

		private const float kSceneButtonHeight = 20.0f;
		private const float kPreviousSceneButtonWidth = 120.0f;

		private static void OnSceneGUI(SceneView sceneView) {
			if (!PrefabSandbox.IsEditing()) {
				return;
			}

			Handles.BeginGUI();

			Color previousColor = GUI.color;

			// BEGIN SCENE GUI
			GUI.color = Color.green;
			if (GUI.Button(new Rect(sceneView.position.size.x - PrefabSandbox.kPreviousSceneButtonWidth, 0.0f, PrefabSandbox.kPreviousSceneButtonWidth, PrefabSandbox.kSceneButtonHeight), "Close Sandbox")) {
				PrefabSandbox.CloseSandboxScene();
			}
			// END SCENE GUI

			GUI.color = previousColor;
			GUI.enabled = true;

			Handles.EndGUI();
		}

		// PRAGMA MARK - Setup
		private static string FindSandboxPath() {
			string guid = AssetDatabaseUtil.FindSpecificAsset(PrefabSandbox.kSandboxSceneName + " t:Scene");
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (PathUtil.IsScene(path)) {
				return path;
			} else {
				throw new Exception(string.Format("Path for sandbox scene ({0}) is not a scene", path));
			}
		}

		private static string FindSandboxSetupPrefabPath() {
			string guid = AssetDatabaseUtil.FindSpecificAsset(PrefabSandbox.kSandboxSetupPrefabName + " t:Prefab");
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (PathUtil.IsPrefab(path)) {
				return path;
			} else {
				throw new Exception(string.Format("Path for sandbox setup prefab ({0}) is not a prefab", path));
			}
		}

		private static void SetupSandbox() {
      PrefabSandbox._sandboxScene = EditorSceneManager.OpenScene(PrefabSandbox.kSandboxScenePath);
      EditorSceneManager.SetActiveScene(PrefabSandbox._sandboxScene);

			if (PrefabSandbox._sandboxScene.isLoaded) {
				PrefabSandbox.ClearAllGameObjectsInSandbox();

				// setup scene with sandbox setup prefab
				GameObject.Instantiate(PrefabSandbox._sandboxSetupPrefab);

				if (!PrefabSandbox.CreatePrefabInstance()) {
					PrefabSandbox.CloseSandboxScene();
					return;
				}
			} else {
				throw new Exception(string.Format("Sandbox Scene ({0}) was not able to be opened!", PrefabSandbox.kSandboxScenePath));
			}
		}

		private static void CloseSandboxScene() {
			if (!PrefabSandbox.IsEditing()) {
				return;
			}

			PrefabSandbox.ClearAllGameObjectsInSandbox();
      PrefabSandbox._sandboxScene = default(Scene);

			EditorSceneManager.OpenScene(PrefabSandbox._data.oldScenePath);
			PrefabSandbox._data = null;
      PrefabSandbox.ClearPrefabData();
		}

		private static bool CreatePrefabInstance() {
			if (PrefabSandbox._data.prefabInstance != null) {
				GameObject.DestroyImmediate(PrefabSandbox._data.prefabInstance);
			}

			PrefabSandbox._data.prefabInstance = PrefabUtility.InstantiatePrefab(PrefabSandbox._data.prefabAsset) as GameObject;

      // if the prefab is a UI element, child it under the canvas
      if (PrefabSandbox._data.prefabInstance.GetComponent<RectTransform>() != null) {
        CanvasUtil.ParentUIElementToCanvas(CanvasUtil.ScreenSpaceMainCanvas, PrefabSandbox._data.prefabInstance);
      }

			Selection.activeGameObject = PrefabSandbox._data.prefabInstance;
      HierarchyUtil.ExpandCurrentSelectedObjectInHierarchy();

			return true;
		}

		private static void ClearAllGameObjectsInSandbox() {
			foreach (GameObject obj in PrefabSandbox._sandboxScene.GetRootGameObjects()) {
				GameObject.DestroyImmediate(obj);
			}
		}

    private static void SavePrefabData() {
      EditorPrefs.SetString("PrefabSandbox._data", JsonUtility.ToJson(PrefabSandbox._data));
    }

    private static void ReloadPrefabData() {
      string serialized = EditorPrefs.GetString("PrefabSandbox._data");
      if (serialized.IsNullOrEmpty()) {
        return;
      }

      Scene currentScene = EditorSceneManager.GetActiveScene();
      if (currentScene.path != PrefabSandbox.kSandboxScenePath) {
        return;
      }

      PrefabSandbox._sandboxScene = currentScene;
      PrefabSandbox._data = JsonUtility.FromJson<PrefabSandboxData>(serialized);
    }

    private static void ClearPrefabData() {
      EditorPrefs.DeleteKey("PrefabSandbox._data");
    }
	}
}
#endif