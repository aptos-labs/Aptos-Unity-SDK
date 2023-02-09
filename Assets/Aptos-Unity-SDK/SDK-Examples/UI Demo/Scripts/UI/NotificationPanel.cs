using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Aptos.Unity.Rest.Model;

namespace Aptos.Unity.Sample.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class NotificationPanel : MonoBehaviour
    {
        [SerializeField] float timer;
        [SerializeField] GameObject errorIcon;
        [SerializeField] GameObject warningIcon;
        [SerializeField] GameObject successIcon;
        [SerializeField] TMP_Text messageText;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = this.GetComponent<CanvasGroup>();
        }

        public void Toggle(ResponseInfo.Status _status, string _message)
        {
            errorIcon.SetActive(false);
            warningIcon.SetActive(false);
            successIcon.SetActive(false);

            switch (_status)
            {
                case ResponseInfo.Status.Success:
                    successIcon.SetActive(true);
                    break;
                case ResponseInfo.Status.Warning:
                    warningIcon.SetActive(true);
                    break;
                case ResponseInfo.Status.Failed:
                    errorIcon.SetActive(true);
                    break;

            }

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
                for (float alpha = 1f; alpha >= 0; alpha -= 0.1f)
                {
                    canvasGroup.alpha = alpha;
                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                for (float alpha = 0f; alpha <= 1; alpha += 0.1f)
                {
                    canvasGroup.alpha = alpha;
                    yield return new WaitForFixedUpdate();
                }
            }
        }
    }
}