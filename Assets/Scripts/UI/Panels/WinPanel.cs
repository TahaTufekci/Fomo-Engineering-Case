using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Panels
{
    public class WinPanel : MonoBehaviour
    {
        public void Next()
        {
            DOTween.KillAll();
            SceneManager.LoadScene(0);
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
