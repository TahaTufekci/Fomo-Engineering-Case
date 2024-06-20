using System.Collections.Generic;
using Helpers;
using JsonReader;
using UnityEngine;

namespace Managers
{
    public class LevelManager : GenericSingleton<LevelManager>
    {
        [SerializeField]private LevelReader levelReader;
        public LevelData CurrentLevel;
        public List<LevelData> allLevels = new List<LevelData>();
        public int CurrentLevelIndex { get; set; } = 0;
        
        void Awake()
        {
            allLevels = levelReader.GetAllLevels();
            LoadLevel();
        }

        private void SetCurrentLevel(LevelData level)
        {
            CurrentLevel = level;
        }
        private void LoadLevel()
        {
            // Check if there's a next level
            if (PlayerPrefs.HasKey("LevelIndex") && PlayerPrefs.GetInt("LevelIndex") < allLevels.Count)
            {
                SetCurrentLevel(allLevels[PlayerPrefs.GetInt("LevelIndex")]);
            }
           
            else
            {
                SetCurrentLevel(allLevels[CurrentLevelIndex]);
            }
        }
        public void SetCurrentLevelIndex(int levelIndex)
        {
            CurrentLevelIndex = levelIndex;
            Debug.Log("Level index set to: " + CurrentLevelIndex);
        }
    }
}
