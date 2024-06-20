using System.Collections.Generic;
using UnityEngine;

namespace JsonReader
{
    public class LevelReader : MonoBehaviour
    {
        [SerializeField] private string levelsFolder = "Levels";
        private List<LevelData> _levels = new List<LevelData>();
    
        public List<LevelData> Levels => _levels;
    
        void Awake() {
            LoadJsonFiles();
        }

        void LoadJsonFiles() {
            // Load all TextAsset files from the specified folder within Resources
            TextAsset[] levelFiles = Resources.LoadAll<TextAsset>(levelsFolder);
        
            foreach (TextAsset levelFile in levelFiles) {
                var levelData = JsonUtility.FromJson<LevelData>(levelFile.text);
                _levels.Add(levelData);
            }

            if (_levels.Count == 0) {
                Debug.LogError("No levels found in the specified folder.");
            }
        }

        public List<LevelData> GetAllLevels() {
            return _levels;
        }
    }
}