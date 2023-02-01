using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Aptos.Unity.Sample.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class NotificationPanel : MonoBehaviour
    {
        [SerializeField] float timer;
        [SerializeField] GameObject errorIcon;
        [SerializeField] GameObject successIcon;
        [SerializeField] TMP_Text messageText;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = this.GetComponent<CanvasGroup>();
        }

        public void Toggle(bool _success, string _message)
        {
            errorIcon.SetActive(!_success);
            successIcon.SetActive(_success);

            messageText.text = _message;

            StartCoroutine(Display());
        }

        IEnumerator Display()
        {
            StartCoroutine(Fade(false));

            yield return new WaitForSeconds(timer);

            Coroutine fadeCor = StartCoroutine(Fade(true));
            yield return fadeCor;

            Destroy(this.gameObject);
        }

        IEnumerator Fade(bool isFade)
        {
            if (isFade)
            {
                for (float alpha = 1f; alpha >= 0; alpha -= 0.01f)
                {
                    canvasGroup.alpha = alpha;
                    yield return null;
                }
            }
            else
            {
                for (float alpha = 0f; alpha <= 1; alpha += 0.01f)
                {
                    canvasGroup.alpha = alpha;
                    yield return null;
                }
            }
        }
    }
}