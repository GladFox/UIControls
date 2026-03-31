using UIControls.Runtime.Controls;
using UIControls.Runtime.Controls.Actions;
using UIControls.Runtime.Core;
using UnityEditor;
using UnityEngine;
using Ease = DG.Tweening.Ease;

namespace UIControls.Editor
{
    public static class UIControlsButtonAnimationLibraryBuilder
    {
        private const string RootFolder = "Assets/UIControls/Animations/UI/Button";
        private const string ProfilesFolder = RootFolder + "/Profiles";
        private const string ActionsFolder = RootFolder + "/Actions";
        private const string TapStatesFolder = RootFolder + "/States/Tap";
        private const string TouchStatesFolder = RootFolder + "/States/Touch";

        [MenuItem("UIControls/Create Button Animation Library")]
        public static void CreateButtonAnimationLibraryFromMenu()
        {
            CreateButtonAnimationLibrary();
        }

        public static void CreateButtonAnimationLibraryBatch()
        {
            CreateButtonAnimationLibrary();
        }

        private static void CreateButtonAnimationLibrary()
        {
            EnsureFolderHierarchy();

            var tapNormal = CreateStateAsset(TapStatesFolder + "/Tap_Normal.asset");
            var tapHover = CreateStateAsset(TapStatesFolder + "/Tap_Hover.asset");
            var tapPressed = CreateStateAsset(TapStatesFolder + "/Tap_Pressed.asset");
            var tapDisabled = CreateStateAsset(TapStatesFolder + "/Tap_Disabled.asset");

            var touchNormal = CreateStateAsset(TouchStatesFolder + "/Touch_Normal.asset");
            var touchHover = CreateStateAsset(TouchStatesFolder + "/Touch_Hover.asset");
            var touchPressed = CreateStateAsset(TouchStatesFolder + "/Touch_Pressed.asset");
            var touchDisabled = CreateStateAsset(TouchStatesFolder + "/Touch_Disabled.asset");

            var normalBlue = new Color(0.03137255f, 0.5803922f, 0.96862745f, 1f);
            var downBlue = new Color(0.03373701f, 0.46191987f, 0.7647059f, 1f);

            ConfigureState(tapNormal, 1f, 1f, normalBlue, 0.1f, Ease.OutQuad);
            ConfigureState(tapHover, 1f, 1f, normalBlue, 0.1f, Ease.OutQuad);
            ConfigureState(tapPressed, 1f, 1f, downBlue, 0.2f, Ease.OutQuad);
            ConfigureState(tapDisabled, 1f, 0.5f, normalBlue, 0.1f, Ease.OutQuad);

            ConfigureState(touchNormal, 1f, 1f, normalBlue, 0.1f, Ease.OutQuad);
            ConfigureState(touchHover, 1f, 1f, normalBlue, 0.1f, Ease.OutQuad);
            ConfigureState(touchPressed, 1f, 1f, normalBlue, 0.1f, Ease.OutQuad);
            ConfigureState(touchDisabled, 1f, 0.5f, normalBlue, 0.1f, Ease.OutQuad);

            var tapDownOffset = LoadOrCreateAsset<UIButtonAnchoredOffsetAction>(ActionsFolder + "/Tap_DownOffset.action.asset");
            var tapClickPulse = LoadOrCreateAsset<UIButtonScalePulseAction>(ActionsFolder + "/Tap_ClickPulse.action.asset");
            var touchDownPulse = LoadOrCreateAsset<UIButtonScalePulseAction>(ActionsFolder + "/Touch_DownPulse.action.asset");

            ConfigureOffsetAction(
                tapDownOffset,
                UIButtonActionTriggerFlags.PointerDown,
                UIButtonActionTriggerFlags.PointerUp | UIButtonActionTriggerFlags.PointerExit | UIButtonActionTriggerFlags.Submit,
                new Vector2(0f, -10f),
                0.2f,
                0.1f,
                Ease.OutQuad,
                Ease.OutQuad);

            ConfigureScalePulseAction(
                tapClickPulse,
                UIButtonActionTriggerFlags.Click | UIButtonActionTriggerFlags.Submit,
                1.1f,
                0.1f,
                0.1f,
                Ease.Linear,
                Ease.Linear);

            ConfigureScalePulseAction(
                touchDownPulse,
                UIButtonActionTriggerFlags.PointerDown,
                1.1f,
                0.1f,
                0.1f,
                Ease.Linear,
                Ease.Linear);

            var tapProfile = LoadOrCreateAsset<UIButtonAnimationProfile>(ProfilesFolder + "/TapProfile.asset");
            var touchProfile = LoadOrCreateAsset<UIButtonAnimationProfile>(ProfilesFolder + "/TouchProfile.asset");

            ConfigureProfile(tapProfile, tapNormal, tapHover, tapPressed, tapDisabled, tapDownOffset, tapClickPulse);
            ConfigureProfile(touchProfile, touchNormal, touchHover, touchPressed, touchDisabled, touchDownPulse);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Button animation library created: {RootFolder}");
        }

        private static UIStateVisualAsset CreateStateAsset(string path)
        {
            return LoadOrCreateAsset<UIStateVisualAsset>(path);
        }

        private static T LoadOrCreateAsset<T>(string path)
            where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void ConfigureState(UIStateVisualAsset asset, float scale, float alpha, Color color, float duration, Ease ease)
        {
            if (asset == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(asset);
            var stateProperty = serializedObject.FindProperty("state");

            stateProperty.FindPropertyRelative("scale").floatValue = scale;
            stateProperty.FindPropertyRelative("alpha").floatValue = alpha;
            stateProperty.FindPropertyRelative("color").colorValue = color;

            var tweenProperty = stateProperty.FindPropertyRelative("tween");
            tweenProperty.FindPropertyRelative("duration").floatValue = duration;
            tweenProperty.FindPropertyRelative("ease").intValue = (int)ease;
            tweenProperty.FindPropertyRelative("delay").floatValue = 0f;
            tweenProperty.FindPropertyRelative("independentUpdate").boolValue = false;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void ConfigureOffsetAction(
            UIButtonAnchoredOffsetAction asset,
            UIButtonActionTriggerFlags pressTriggers,
            UIButtonActionTriggerFlags releaseTriggers,
            Vector2 pressedOffset,
            float pressDuration,
            float releaseDuration,
            Ease pressEase,
            Ease releaseEase)
        {
            if (asset == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("pressTriggers").intValue = (int)pressTriggers;
            serializedObject.FindProperty("releaseTriggers").intValue = (int)releaseTriggers;
            serializedObject.FindProperty("releaseWhenDisabled").boolValue = true;
            serializedObject.FindProperty("pressedOffset").vector2Value = pressedOffset;
            serializedObject.FindProperty("pressDuration").floatValue = pressDuration;
            serializedObject.FindProperty("releaseDuration").floatValue = releaseDuration;
            serializedObject.FindProperty("pressEase").intValue = (int)pressEase;
            serializedObject.FindProperty("releaseEase").intValue = (int)releaseEase;
            serializedObject.FindProperty("independentUpdate").boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void ConfigureScalePulseAction(
            UIButtonScalePulseAction asset,
            UIButtonActionTriggerFlags triggerFlags,
            float scaleMultiplier,
            float scaleUpDuration,
            float scaleDownDuration,
            Ease scaleUpEase,
            Ease scaleDownEase)
        {
            if (asset == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("triggerFlags").intValue = (int)triggerFlags;
            serializedObject.FindProperty("scaleMultiplier").floatValue = scaleMultiplier;
            serializedObject.FindProperty("scaleUpDuration").floatValue = scaleUpDuration;
            serializedObject.FindProperty("scaleDownDuration").floatValue = scaleDownDuration;
            serializedObject.FindProperty("scaleUpEase").intValue = (int)scaleUpEase;
            serializedObject.FindProperty("scaleDownEase").intValue = (int)scaleDownEase;
            serializedObject.FindProperty("independentUpdate").boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void ConfigureProfile(
            UIButtonAnimationProfile profile,
            UIStateVisualAsset normal,
            UIStateVisualAsset hover,
            UIStateVisualAsset pressed,
            UIStateVisualAsset disabled,
            params UIButtonCustomAction[] actions)
        {
            if (profile == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(profile);
            serializedObject.FindProperty("normal").objectReferenceValue = normal;
            serializedObject.FindProperty("hover").objectReferenceValue = hover;
            serializedObject.FindProperty("pressed").objectReferenceValue = pressed;
            serializedObject.FindProperty("disabled").objectReferenceValue = disabled;

            var actionsProperty = serializedObject.FindProperty("customActions");
            actionsProperty.arraySize = actions.Length;
            for (var i = 0; i < actions.Length; i++)
            {
                actionsProperty.GetArrayElementAtIndex(i).objectReferenceValue = actions[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureFolderHierarchy()
        {
            EnsureFolder("Assets/UIControls");
            EnsureFolder("Assets/UIControls/Animations");
            EnsureFolder("Assets/UIControls/Animations/UI");
            EnsureFolder(RootFolder);
            EnsureFolder(ProfilesFolder);
            EnsureFolder(ActionsFolder);
            EnsureFolder("Assets/UIControls/Animations/UI/Button/States");
            EnsureFolder(TapStatesFolder);
            EnsureFolder(TouchStatesFolder);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
