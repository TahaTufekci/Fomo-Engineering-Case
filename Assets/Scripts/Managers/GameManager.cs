using System;
using Builders;
using States;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Actions
        public Action<GameState> OnGameStateChanged;
        public Action<BlockCell,Vector2> OnBlockSwiped;
        public Action OnValidMove;
        public Action OnMoveNumberDecrease;
        #endregion
        public GameState currentGameState;
        private int _moveNumber;
        [SerializeField] private GridBuilder gridBuilder;
        public static GameManager Instance;
        
        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
                Instance = this;

            currentGameState = GameState.WaitingInput;
            _moveNumber = LevelManager.Instance.CurrentLevel.MoveLimit == 0 ? int.MaxValue : LevelManager.Instance.CurrentLevel.MoveLimit;
        }

        public void ChangeGameState(GameState state)
        {
            if (currentGameState != state)
            {
                currentGameState = state;
                OnGameStateChanged?.Invoke(state);
            }
        }
        public int GetCurrentMoveCount()
        {
            return _moveNumber;
        }
        private void UpdateCurrentMoveNumber()
        {
            --_moveNumber;
            OnMoveNumberDecrease?.Invoke(); 
            if (gridBuilder.BlockList.Count <= 0)
            {
                if(PlayerPrefs.GetInt("LevelIndex") == LevelManager.Instance.allLevels.Count - 1)
                {
                    ChangeGameState(GameState.Finish);
                }
                else if (PlayerPrefs.HasKey("LevelIndex"))
                {
                    PlayerPrefs.SetInt("LevelIndex",PlayerPrefs.GetInt("LevelIndex") + 1);
                    ChangeGameState(GameState.Win);
                }
                else
                {
                    PlayerPrefs.SetInt("LevelIndex",++LevelManager.Instance.CurrentLevelIndex);
                    ChangeGameState(GameState.Win);
                }
            }
            if(_moveNumber == 0)
            {
                ChangeGameState(GameState.Lose);
            }
        }

        private void OnEnable()
        {
            OnValidMove += UpdateCurrentMoveNumber;
        }

        private void OnDisable()
        {
            OnValidMove -= UpdateCurrentMoveNumber;
        }
    }
}
