using UIControls.Runtime.Controls;
using UnityEditor;
using UnityEngine;

namespace UIControls.Editor
{
    [CustomEditor(typeof(UIProgressBarControl))]
    [CanEditMultipleObjects]
    public sealed class UIProgressBarControlEditor : UnityEditor.Editor
    {
        // Base
        SerializedProperty _fillImage;
        SerializedProperty _fillMode;
        SerializedProperty _valueLabel;
        SerializedProperty _valueFormat;
        SerializedProperty _value;
        SerializedProperty _animateLabel;
        SerializedProperty _maxValue;
        SerializedProperty _tween;

        // Segments
        SerializedProperty _useSegments;
        SerializedProperty _segmentsCount;
        SerializedProperty _autoGenerateSegments;
        SerializedProperty _segmentVisualMode;
        SerializedProperty _generatedSegmentsRoot;
        SerializedProperty _segmentGap;
        SerializedProperty _dividerWidth;
        SerializedProperty _dividerColor;
        SerializedProperty _segmentFillSprite;
        SerializedProperty _segmentDividerSprite;
        SerializedProperty _segmentFills;
        SerializedProperty _fillColor;
        SerializedProperty _segmentCompletedColor;
        SerializedProperty _triggerControlStateOnSegmentCompleted;
        SerializedProperty _controlSegmentStateAnimator;
        SerializedProperty _controlSegmentCompletedState;
        SerializedProperty _triggerSegmentStateOnSegmentCompleted;
        SerializedProperty _segmentPulse;

        // HitBar
        SerializedProperty _useHitBar;
        SerializedProperty _primaryFillImage;
        SerializedProperty _echoFillImage;
        SerializedProperty _primaryDropDuration;
        SerializedProperty _echoDelay;
        SerializedProperty _echoDuration;
        SerializedProperty _echoEase;
        SerializedProperty _increaseMode;
        SerializedProperty _hideEchoOnIncrease;
        SerializedProperty _useEchoTimingOnIncrease;

        // Events
        SerializedProperty _onValueChanged;
        SerializedProperty _onSegmentCompleted;
        SerializedProperty _onEchoStarted;
        SerializedProperty _onEchoCompleted;

        // Custom Actions
        SerializedProperty _customActions;

        void OnEnable()
        {
            _fillImage   = serializedObject.FindProperty("fillImage");
            _fillMode    = serializedObject.FindProperty("fillMode");
            _valueLabel  = serializedObject.FindProperty("valueLabel");
            _valueFormat = serializedObject.FindProperty("valueFormat");
            _value       = serializedObject.FindProperty("value");
            _animateLabel = serializedObject.FindProperty("animateLabel");
            _maxValue    = serializedObject.FindProperty("maxValue");
            _tween       = serializedObject.FindProperty("tween");

            _useSegments          = serializedObject.FindProperty("useSegments");
            _segmentsCount        = serializedObject.FindProperty("segmentsCount");
            _autoGenerateSegments = serializedObject.FindProperty("autoGenerateSegments");
            _segmentVisualMode    = serializedObject.FindProperty("segmentVisualMode");
            _generatedSegmentsRoot    = serializedObject.FindProperty("generatedSegmentsRoot");
            _segmentGap           = serializedObject.FindProperty("segmentGap");
            _dividerWidth         = serializedObject.FindProperty("dividerWidth");
            _dividerColor         = serializedObject.FindProperty("dividerColor");
            _segmentFillSprite    = serializedObject.FindProperty("segmentFillSprite");
            _segmentDividerSprite = serializedObject.FindProperty("segmentDividerSprite");
            _segmentFills         = serializedObject.FindProperty("segmentFills");
            _fillColor            = serializedObject.FindProperty("fillColor");
            _segmentCompletedColor                  = serializedObject.FindProperty("segmentCompletedColor");
            _triggerControlStateOnSegmentCompleted  = serializedObject.FindProperty("triggerControlStateOnSegmentCompleted");
            _controlSegmentStateAnimator            = serializedObject.FindProperty("controlSegmentStateAnimator");
            _controlSegmentCompletedState           = serializedObject.FindProperty("controlSegmentCompletedState");
            _triggerSegmentStateOnSegmentCompleted  = serializedObject.FindProperty("triggerSegmentStateOnSegmentCompleted");
            _segmentPulse         = serializedObject.FindProperty("segmentPulse");

            _useHitBar             = serializedObject.FindProperty("useHitBar");
            _primaryFillImage      = serializedObject.FindProperty("primaryFillImage");
            _echoFillImage         = serializedObject.FindProperty("echoFillImage");
            _primaryDropDuration   = serializedObject.FindProperty("primaryDropDuration");
            _echoDelay             = serializedObject.FindProperty("echoDelay");
            _echoDuration          = serializedObject.FindProperty("echoDuration");
            _echoEase              = serializedObject.FindProperty("echoEase");
            _increaseMode          = serializedObject.FindProperty("increaseMode");
            _hideEchoOnIncrease    = serializedObject.FindProperty("hideEchoOnIncrease");
            _useEchoTimingOnIncrease = serializedObject.FindProperty("useEchoTimingOnIncrease");

            _onValueChanged    = serializedObject.FindProperty("onValueChanged");
            _onSegmentCompleted = serializedObject.FindProperty("onSegmentCompleted");
            _onEchoStarted     = serializedObject.FindProperty("onEchoStarted");
            _onEchoCompleted   = serializedObject.FindProperty("onEchoCompleted");

            _customActions = serializedObject.FindProperty("customActions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawBase();
            EditorGUILayout.Space(2f);
            DrawSegments();
            EditorGUILayout.Space(2f);
            DrawHitBar();
            EditorGUILayout.Space(2f);
            DrawEvents();
            EditorGUILayout.Space(2f);
            DrawCustomActions();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawBase()
        {
            EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_fillImage);
            EditorGUILayout.PropertyField(_fillMode);
            EditorGUILayout.PropertyField(_value);
            EditorGUILayout.PropertyField(_tween);

            EditorGUILayout.Space(2f);
            EditorGUILayout.PropertyField(_valueLabel);
            if (_valueLabel.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_maxValue);
                EditorGUILayout.PropertyField(_valueFormat);
                EditorGUILayout.HelpBox("{0} = normalized (0–1)   {1} = current × maxValue   {2} = maxValue\nExamples:  {0:0%} → \"75%\"     {1:0}/{2:0} → \"750/1000\"", MessageType.None);
                EditorGUILayout.PropertyField(_animateLabel);
                EditorGUI.indentLevel--;
            }
        }

        void DrawSegments()
        {
            EditorGUILayout.LabelField("Segments", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useSegments);

            if (!_useSegments.boolValue)
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(_segmentsCount);
            EditorGUILayout.PropertyField(_fillColor);
            EditorGUILayout.PropertyField(_segmentCompletedColor);
            EditorGUILayout.PropertyField(_autoGenerateSegments);
            EditorGUILayout.PropertyField(_segmentVisualMode);

            var autoGen = _autoGenerateSegments.boolValue;
            var isFillBlocks = _segmentVisualMode.enumValueIndex == 0; // FillBlocks = 0

            if (autoGen)
            {
                EditorGUILayout.PropertyField(_generatedSegmentsRoot);
                if (isFillBlocks)
                {
                    EditorGUILayout.PropertyField(_segmentGap);
                    EditorGUILayout.PropertyField(_segmentFillSprite);
                }
                EditorGUILayout.PropertyField(_dividerWidth);
                if (_dividerWidth.floatValue > 0f)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_dividerColor);
                    EditorGUILayout.PropertyField(_segmentDividerSprite);
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(_segmentFills);
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.PropertyField(_triggerControlStateOnSegmentCompleted);
            if (_triggerControlStateOnSegmentCompleted.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_controlSegmentStateAnimator);
                EditorGUILayout.PropertyField(_controlSegmentCompletedState);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_triggerSegmentStateOnSegmentCompleted);
            if (_triggerSegmentStateOnSegmentCompleted.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_segmentPulse);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        void DrawHitBar()
        {
            EditorGUILayout.LabelField("HitBar", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useHitBar);

            if (!_useHitBar.boolValue)
            {
                // Hide the EchoFill object in the hierarchy when HitBar is off
                SyncEchoFillObjectActive(false);
                return;
            }

            SyncEchoFillObjectActive(true);

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(_primaryFillImage);
            EditorGUILayout.PropertyField(_echoFillImage);
            EditorGUILayout.PropertyField(_primaryDropDuration);

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Echo", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(_echoDelay);
            EditorGUILayout.PropertyField(_echoDuration);
            EditorGUILayout.PropertyField(_echoEase);

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Increase behaviour", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(_increaseMode);
            EditorGUILayout.PropertyField(_hideEchoOnIncrease);

            // useEchoTimingOnIncrease only matters when echo is visible on increase
            var echoVisibleOnIncrease = !_hideEchoOnIncrease.boolValue &&
                                        _increaseMode.enumValueIndex == 0; // SyncBoth = 0
            using (new EditorGUI.DisabledScope(!echoVisibleOnIncrease))
            {
                EditorGUILayout.PropertyField(_useEchoTimingOnIncrease);
            }

            EditorGUI.indentLevel--;
        }

        void DrawEvents()
        {
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onValueChanged);
            if (_useSegments.boolValue)
            {
                EditorGUILayout.PropertyField(_onSegmentCompleted);
            }
            if (_useHitBar.boolValue)
            {
                EditorGUILayout.PropertyField(_onEchoStarted);
                EditorGUILayout.PropertyField(_onEchoCompleted);
            }
        }

        void DrawCustomActions()
        {
            EditorGUILayout.PropertyField(_customActions);
        }

        void SyncEchoFillObjectActive(bool active)
        {
            // Only act when editing a single object to avoid ambiguous multi-edit
            if (targets.Length != 1)
            {
                return;
            }

            var echoRef = _echoFillImage.objectReferenceValue as UnityEngine.UI.Image;
            if (echoRef == null)
            {
                return;
            }

            var go = echoRef.gameObject;
            if (go.activeSelf == active)
            {
                return;
            }

            Undo.RecordObject(go, active ? "Enable EchoFill" : "Disable EchoFill");
            go.SetActive(active);
        }
    }
}
