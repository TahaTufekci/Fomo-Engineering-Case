using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainUI
{
    public class MainPlayButton : MonoBehaviour
    {
        public void PlayGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
