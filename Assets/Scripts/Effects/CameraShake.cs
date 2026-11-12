using System.Collections;
using UnityEngine;

namespace WallBreaker.Effects
{
    public class CameraShake : MonoBehaviour
    {
        private Coroutine shakeRoutine;

        public void Shake(float duration, float strength)
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            Vector3 originalLocalPosition = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                Vector2 randomOffset = Random.insideUnitCircle * strength;
                transform.localPosition = originalLocalPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

                yield return null;
            }

            transform.localPosition = originalLocalPosition;
            shakeRoutine = null;
        }
    }
}
