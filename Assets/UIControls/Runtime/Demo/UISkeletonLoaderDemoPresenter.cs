using System.Collections;
using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UISkeletonLoaderDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UISkeletonLoaderControl skeleton;
        [SerializeField] private UIButtonControl reloadButton;
        [SerializeField] private TMP_Text statusLabel;
        [Min(0f)]
        [SerializeField] private float loadSeconds = 1.8f;

        private void OnEnable()
        {
            if (reloadButton != null)
            {
                reloadButton.OnClick.AddListener(Reload);
            }

            Reload();
        }

        private void OnDisable()
        {
            if (reloadButton != null)
            {
                reloadButton.OnClick.RemoveListener(Reload);
            }
        }

        private void Reload()
        {
            StopAllCoroutines();
            StartCoroutine(LoadRoutine());
        }

        private IEnumerator LoadRoutine()
        {
            if (skeleton != null)
            {
                skeleton.SetLoading(true);
            }

            SetStatus("Loading…");
            yield return new WaitForSecondsRealtime(loadSeconds);

            if (skeleton != null)
            {
                skeleton.SetLoading(false);
            }

            SetStatus("Loaded. Press Reload to replay the skeleton.");
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null)
            {
                statusLabel.text = text;
            }
        }
    }
}
