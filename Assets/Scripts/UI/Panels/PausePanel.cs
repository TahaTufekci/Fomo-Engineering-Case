using DG.Tweening;
using Managers;
using States;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Panels
{
    public class PausePanel : MonoBehaviour
    {
        public void Continue()
        {
            DOTween.KillAll();
            gameObject.SetActive(false);
            GameManager.Instance.ChangeGameState(GameState.WaitingInput);
        }

        public void ReturnMenu()
        {
            DOTween.KillAll();
            SceneManager.LoadScene(0);
        }
        public void PauseTheGame()
        {
            GameManager.Instance.ChangeGameState(GameState.Pause);
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
