using TMPro;
using UIControls.Runtime.Controls;
using UnityEngine;

namespace UIControls.Runtime.Demo
{
    public sealed class UISwipeCardDemoPresenter : MonoBehaviour
    {
        [SerializeField] private UISwipeCardControl card;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text roleLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private UIButtonControl likeButton;
        [SerializeField] private UIButtonControl nopeButton;

        private readonly (string Name, string Role)[] deck =
        {
            ("Aria Nakamura", "Gameplay Engineer"),
            ("Leo Marsh", "Technical Artist"),
            ("Priya Sundar", "Producer"),
            ("Dmitri Volkov", "Audio Designer"),
            ("Mira Castillo", "UX Designer"),
        };

        private int index;
        private int likes;
        private int passes;

        private void OnEnable()
        {
            index = 0;
            likes = 0;
            passes = 0;
            if (card != null) card.OnSwipe.AddListener(HandleSwipe);
            if (likeButton != null) likeButton.OnClick.AddListener(Like);
            if (nopeButton != null) nopeButton.OnClick.AddListener(Nope);
            Populate();
        }

        private void OnDisable()
        {
            if (card != null) card.OnSwipe.RemoveListener(HandleSwipe);
            if (likeButton != null) likeButton.OnClick.RemoveListener(Like);
            if (nopeButton != null) nopeButton.OnClick.RemoveListener(Nope);
        }

        private void Like() => card?.Swipe(1);

        private void Nope() => card?.Swipe(-1);

        private void HandleSwipe(int direction)
        {
            if (direction > 0) likes++; else passes++;
            index++;

            if (index >= deck.Length)
            {
                if (card != null) card.gameObject.SetActive(false);
                if (statusLabel != null) statusLabel.text = $"Done — {likes} liked, {passes} passed.";
                return;
            }

            if (card != null) card.ResetCard();
            Populate();
        }

        private void Populate()
        {
            if (index < deck.Length)
            {
                if (nameLabel != null) nameLabel.text = deck[index].Name;
                if (roleLabel != null) roleLabel.text = deck[index].Role;
            }

            if (statusLabel != null)
            {
                statusLabel.text = $"Swipe right to like, left to pass.  ({likes} liked, {passes} passed)";
            }
        }
    }
}
