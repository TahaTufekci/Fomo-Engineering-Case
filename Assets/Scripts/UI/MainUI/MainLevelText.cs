using Managers;
using TMPro;
using UnityEngine;

namespace UI.MainUI
{
    public class MainLevelText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelNoText;

        // Start is called before the first frame update
        void Start()
        {
            var currentLevelNo = LevelManager.Instance.allLevels.IndexOf(LevelManager.Instance.CurrentLevel) + 1;
            levelNoText.text = $"{currentLevelNo.ToString()}"; 
        }

    }
}
