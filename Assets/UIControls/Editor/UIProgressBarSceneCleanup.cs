using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIControls.Editor
{
    /// <summary>
    /// Removes orphaned <c>AutoSegments</c> / <c>AutoSegment_*</c> / <c>AutoDivider_*</c>
    /// objects that older builds of <c>UIProgressBarControl</c> stranded in the scene
    /// root. Only root-level objects are touched — the ones living under a progress bar
    /// are legitimate generated visuals and are left alone.
    /// </summary>
    public static class UIProgressBarSceneCleanup
    {
        private const string AutoSegmentsRootName = "AutoSegments";
        private const string AutoSegmentPrefix = "AutoSegment_";
        private const string AutoDividerPrefix = "AutoDivider_";

        [MenuItem("UIControls/Cleanup Orphaned ProgressBar Objects (open scenes)")]
        public static void CleanupOpenScenes()
        {
            var removed = 0;
            var scenesTouched = 0;

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                {
                    continue;
                }

                var removedHere = CleanupScene(scene);
                if (removedHere > 0)
                {
                    removed += removedHere;
                    scenesTouched++;
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            if (removed == 0)
            {
                Debug.Log("[UIControls] No orphaned ProgressBar objects found in the open scenes.");
                return;
            }

            Debug.Log($"[UIControls] Removed {removed} orphaned ProgressBar object(s) " +
                      $"from {scenesTouched} scene(s). Save the scene(s) to persist the cleanup.");
        }

        private static int CleanupScene(Scene scene)
        {
            var orphans = new List<GameObject>();

            foreach (var root in scene.GetRootGameObjects())
            {
                if (IsGenerated(root.name))
                {
                    orphans.Add(root);
                }
            }

            for (var i = 0; i < orphans.Count; i++)
            {
                Undo.DestroyObjectImmediate(orphans[i]);
            }

            return orphans.Count;
        }

        private static bool IsGenerated(string objectName)
        {
            return objectName == AutoSegmentsRootName ||
                   objectName.StartsWith(AutoSegmentPrefix, System.StringComparison.Ordinal) ||
                   objectName.StartsWith(AutoDividerPrefix, System.StringComparison.Ordinal);
        }
    }
}
