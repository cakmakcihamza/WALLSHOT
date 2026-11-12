using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using WallBreaker.Wall;

namespace WallBreaker.UI
{
    public class WallProgressUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        private int totalBricks;
        private int activatedBricks;

        private void OnEnable()
        {
            BrickPhysicsActivator.BrickActivated += HandleBrickActivated;
            StartCoroutine(InitializeAfterWallGeneration());
        }

        private void OnDisable()
        {
            BrickPhysicsActivator.BrickActivated -= HandleBrickActivated;
        }

        public void Initialize(Image progressFillImage)
        {
            fillImage = progressFillImage;
            UpdateFill();
        }

        private IEnumerator InitializeAfterWallGeneration()
        {
            yield return null;

            BrickPhysicsActivator[] bricks = FindObjectsByType<BrickPhysicsActivator>(FindObjectsSortMode.None);
            totalBricks = bricks.Length;
            activatedBricks = 0;

            for (int i = 0; i < bricks.Length; i++)
            {
                if (bricks[i].IsActivated)
                {
                    activatedBricks++;
                }
            }

            UpdateFill();
        }

        private void HandleBrickActivated(BrickPhysicsActivator brick)
        {
            if (totalBricks <= 0)
            {
                return;
            }

            activatedBricks = Mathf.Min(activatedBricks + 1, totalBricks);
            UpdateFill();
        }

        private void UpdateFill()
        {
            if (fillImage == null)
            {
                return;
            }

            float progress = totalBricks > 0 ? Mathf.Clamp01((float)activatedBricks / totalBricks) : 0f;
            fillImage.fillAmount = progress;
        }
    }
}
