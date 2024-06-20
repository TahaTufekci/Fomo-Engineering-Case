using DG.Tweening;
using UnityEngine;

namespace UI.Panels
{
    public class FinishPanel : MonoBehaviour
    {
        public void Quit()
        {
            DOTween.KillAll();
            Application.Quit();
        }

        public void OnEnable()
        {
            EnableAnimation();
        }

        private void EnableAnimation()
        {
            gameObject.transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero);
        }
    }
}
