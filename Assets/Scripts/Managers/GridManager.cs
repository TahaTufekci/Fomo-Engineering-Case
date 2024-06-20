using System.Collections;
using System.Collections.Generic;
using Builders;
using DG.Tweening;
using Helpers;
using Objects;
using UnityEngine;

namespace Managers
{
    public class GridManager : MonoBehaviour
    {
        private LevelData _currentLevel;
        private List<Cell> _cellList = new List<Cell>();
        private List<ExitGate> _exitList = new List<ExitGate>();
        
        [SerializeField] private List<ParticleSystem> particleSystemPrefab;
        [SerializeField] private GridBuilder gridBuilder;
        
        public float moveDuration = 0.3f; // Duration of the movement animation
        
        private void Start()
        {
            _currentLevel = LevelManager.Instance.CurrentLevel;
            _cellList = gridBuilder.CellList;
            _exitList = gridBuilder.ExitList;
        }

        private void OnBlockSwiped(BlockCell selectedBlock, Vector2 swipeDirection)
        {
            if (selectedBlock == null)
                return;
            var direction = DirectionMapper.GetDirection(swipeDirection);
            if (selectedBlock.directions.Contains(direction))
            {
                if (direction is 1 or 3)
                {
                    HandleHorizontalMovement(selectedBlock, swipeDirection);
                }
                else
                {
                    HandleVerticalMovement(selectedBlock, swipeDirection);
                }
            }
            else
            {
                Debug.Log("Block cannot move in the swipe direction");
            }
        }
        private void HandleVerticalMovement(BlockCell selectedBlock, Vector2 swipeDirection)
        {
            if (swipeDirection == Vector2.down)
            {
                MoveBlockDown(selectedBlock);
            }
            else
            {
                MoveBlockUp(selectedBlock);
            }
        }
        private void MoveBlockVertically(BlockCell selectedBlock, int checkIndex, Vector2 direction)
        {
            Cell targetCell;
            if (direction == Vector2.up)
            {
                targetCell = GetCell(selectedBlock.posX, ++checkIndex);
            }
            else
            {
                targetCell = GetCell(selectedBlock.posX, --checkIndex);
            }
            selectedBlock.MoveToTarget(targetCell.transform.position);
            selectedBlock.SetCoordinates(targetCell.posX, targetCell.posY);
        }
        private void MoveBlockUp(BlockCell selectedBlock)
        {
            var checkIndex = selectedBlock.posY - 1;
            var numOfCellPassed = 0;
            while (checkIndex > -1 && GetCell(selectedBlock.posX, checkIndex).cellSituation == CellSituation.Empty)
            {
                numOfCellPassed++;
                checkIndex--;
            }

            if (checkIndex > -1 && GetCell(selectedBlock.posX, checkIndex).cellSituation ==
                CellSituation.HasMovableBlock)
            {
                if (numOfCellPassed == 0) return;

                if (numOfCellPassed >= selectedBlock.length)
                {
                    UpdateVerticalBlockStatus(selectedBlock,CellSituation.Empty);
                }
                else
                {
                    UpdatePassedVerticalCells(selectedBlock,CellSituation.Empty,numOfCellPassed);
                }
                
                MoveBlockVertically(selectedBlock, checkIndex, Vector2.up);

               UpdateVerticalBlockStatus(selectedBlock,CellSituation.HasMovableBlock);

                GameManager.Instance.OnValidMove?.Invoke();
            }
            else
            {
                var exitgate = GetExitGate(selectedBlock.posX, 0, selectedBlock.directions);
                if (exitgate.colorValue == selectedBlock.colorValue)
                {
                    UpdateVerticalBlockStatus(selectedBlock,CellSituation.Empty);

                    StartCoroutine(MoveAndDestroyVertically(selectedBlock,exitgate));

                    GameManager.Instance.OnValidMove?.Invoke();
                }
                else
                {
                    if (numOfCellPassed == 0) return;
                    
                    UpdateVerticalBlockStatus(selectedBlock,CellSituation.Empty);
                    
                    MoveBlockVertically(selectedBlock, checkIndex, Vector2.up);

                    UpdateVerticalBlockStatus(selectedBlock,CellSituation.HasMovableBlock);

                    GameManager.Instance.OnValidMove?.Invoke();
                }
            }
        }
        private void MoveBlockDown(BlockCell selectedBlock)
        {
            var checkIndex = selectedBlock.posY + selectedBlock.length;
            var numOfCellPassed = 0;
            while (checkIndex < _currentLevel.RowCount &&
                   GetCell(selectedBlock.posX, checkIndex).cellSituation == CellSituation.Empty)
            {
                numOfCellPassed++;
                checkIndex++;
            }

            if (checkIndex < _currentLevel.ColCount && GetCell(selectedBlock.posX, checkIndex).cellSituation ==
                CellSituation.HasMovableBlock)
            {
                if (numOfCellPassed == 0) return;

                if (numOfCellPassed >= selectedBlock.length)
                {
                    UpdateVerticalBlockStatus(selectedBlock,CellSituation.Empty);
                }
                else
                {
                    UpdatePassedVerticalCells(selectedBlock,CellSituation.Empty,numOfCellPassed);
                }

                if (selectedBlock.length > 1)
                {
                    checkIndex -= selectedBlock.length - 1;
                }

                MoveBlockVertically(selectedBlock, checkIndex, Vector2.down);
                UpdateVerticalBlockStatus(selectedBlock,CellSituation.HasMovableBlock);
                GameManager.Instance.OnValidMove?.Invoke();
            }
            else
            {
                var exitgate = GetExitGate(selectedBlock.posX, _currentLevel.RowCount - 1,
                    selectedBlock.directions);
                if (exitgate.colorValue == selectedBlock.colorValue)
                {
                    UpdateVerticalBlockStatus(selectedBlock,CellSituation.Empty);

                    StartCoroutine(MoveAndDestroyVertically(selectedBlock,exitgate));

                    GameManager.Instance.OnValidMove?.Invoke();
                }
                else
                {
                    if (numOfCellPassed == 0) return;

                    UpdateVerticalBlockStatus(selectedBlock,CellSituation.Empty);

                    if (selectedBlock.length > 1)
                    {
                        checkIndex -= selectedBlock.length - 1;
                    }

                    MoveBlockVertically(selectedBlock, checkIndex, Vector2.down);
                    UpdateVerticalBlockStatus(selectedBlock,CellSituation.HasMovableBlock);

                    GameManager.Instance.OnValidMove?.Invoke();
                }
            }
        }
        private void UpdateVerticalBlockStatus(BlockCell selectedBlock, CellSituation cellSituation)
        {
            for (var i = 0; i < selectedBlock.length; i++)
            {
                var finalBlockCell = GetCell(selectedBlock.posX, selectedBlock.posY + i);
                finalBlockCell.cellSituation = cellSituation;
                finalBlockCell.block = null;
            }
        }
        private void UpdatePassedVerticalCells(BlockCell selectedBlock, CellSituation cellSituation, int numOfCellPassed)
        {
            for (var i = 0; i < numOfCellPassed; i++)
            {
                var finalBlockCell = GetCell(selectedBlock.posX, selectedBlock.posY + i);
                finalBlockCell.cellSituation = cellSituation;
                finalBlockCell.block = null;
            }
        }
        private void HandleHorizontalMovement(BlockCell selectedBlock, Vector2 swipeDirection)
        {
            if (swipeDirection == Vector2.right)
            {
                MoveBlockRight(selectedBlock, swipeDirection);
            }
            else
            {
                MoveBlockLeft(selectedBlock);
            }
        }
        private void MoveBlockHorizontally(BlockCell selectedBlock, int checkIndex, Vector2 direction)
        {
            Cell targetCell;
            if (direction == Vector2.left)
            {
                targetCell = GetCell(++checkIndex, selectedBlock.posY);
            }
            else
            {
                targetCell = GetCell(--checkIndex, selectedBlock.posY);
            }
            selectedBlock.MoveToTarget(targetCell.transform.position);
            selectedBlock.SetCoordinates(targetCell.posX, targetCell.posY);
        }
        private void MoveBlockLeft(BlockCell selectedBlock)
        {
            var checkIndex = selectedBlock.posX - 1;
            var numOfCellPassed = 0;

            while (checkIndex > -1 && GetCell(checkIndex, selectedBlock.posY).cellSituation == CellSituation.Empty)
            {
                numOfCellPassed++;
                checkIndex--;
            }

            if (checkIndex > -1 && GetCell(checkIndex, selectedBlock.posY).cellSituation == CellSituation.HasMovableBlock)
            {
                if (numOfCellPassed == 0) return;

                if (numOfCellPassed >= selectedBlock.length)
                {
                    UpdateHorizontalBlockStatus(selectedBlock,CellSituation.Empty);
                }
                else
                {
                    UpdatePassedHorizontalCells(selectedBlock,CellSituation.Empty,numOfCellPassed);
                }
                if (selectedBlock.length > 1)
                {
                    checkIndex -= selectedBlock.length - 1;
                }

                MoveBlockHorizontally(selectedBlock, checkIndex, Vector2.left);
                UpdateHorizontalBlockStatus(selectedBlock,CellSituation.HasMovableBlock);
                GameManager.Instance.OnValidMove?.Invoke();
            }
            else
            {
                var exitgate = GetExitGate(0, selectedBlock.posY, selectedBlock.directions);
                if (exitgate.colorValue == selectedBlock.colorValue)
                {
                    UpdateHorizontalBlockStatus(selectedBlock, CellSituation.Empty);

                    StartCoroutine(MoveAndDestroyHorizontally(selectedBlock,exitgate));
                    GameManager.Instance.OnValidMove?.Invoke();
                }
                else
                {
                    if (numOfCellPassed == 0) return;

                    UpdateHorizontalBlockStatus(selectedBlock, CellSituation.Empty);
                    if (selectedBlock.length > 1)
                    {
                        checkIndex -= selectedBlock.length - 1;
                    }
                    MoveBlockHorizontally(selectedBlock, checkIndex, Vector2.left);
                    UpdateHorizontalBlockStatus(selectedBlock, CellSituation.HasMovableBlock);
                    GameManager.Instance.OnValidMove?.Invoke();
                }
            }
        }
        private void MoveBlockRight(BlockCell selectedBlock, Vector2 swipeDirection)
        {
            var direction = swipeDirection == Vector2.right ? 1 : -1;
            var checkIndex = selectedBlock.posX + direction * selectedBlock.length;
            var numOfCellPassed = 0;

            while (checkIndex < _currentLevel.ColCount &&
                   GetCell(checkIndex, selectedBlock.posY).cellSituation == CellSituation.Empty)
            {
                numOfCellPassed++;
                checkIndex++;
            }

            if (checkIndex < _currentLevel.ColCount &&
                GetCell(checkIndex, selectedBlock.posY).cellSituation == CellSituation.HasMovableBlock)
            {
                if (numOfCellPassed == 0) return;
                if (numOfCellPassed >= selectedBlock.length)
                {
                    UpdateHorizontalBlockStatus(selectedBlock,CellSituation.Empty);
                }
                else
                {
                    UpdatePassedHorizontalCells(selectedBlock,CellSituation.Empty,numOfCellPassed);
                }
                if (selectedBlock.length > 1)
                {
                    checkIndex -= selectedBlock.length - 1;
                }

                MoveBlockHorizontally(selectedBlock, checkIndex, Vector2.right);
                UpdateHorizontalBlockStatus(selectedBlock,CellSituation.HasMovableBlock);
                GameManager.Instance.OnValidMove?.Invoke();
            }
            else
            {
                var exitgate = GetExitGate(_currentLevel.ColCount - 1, selectedBlock.posY, selectedBlock.directions);
                if (exitgate.colorValue == selectedBlock.colorValue)
                {
                    UpdateHorizontalBlockStatus(selectedBlock, CellSituation.Empty);

                    StartCoroutine(MoveAndDestroyHorizontally(selectedBlock,exitgate));
                    GameManager.Instance.OnValidMove?.Invoke();
                }
                else
                {
                    if (numOfCellPassed == 0) return;

                    UpdateHorizontalBlockStatus(selectedBlock, CellSituation.Empty);
                    if (selectedBlock.length > 1)
                    {
                        checkIndex -= selectedBlock.length - 1;
                    }
                    MoveBlockHorizontally(selectedBlock, checkIndex, Vector2.right);
                    UpdateHorizontalBlockStatus(selectedBlock, CellSituation.HasMovableBlock);
                    GameManager.Instance.OnValidMove?.Invoke();
                }
            }
        }
        private void UpdatePassedHorizontalCells(BlockCell selectedBlock, CellSituation cellSituation, int numOfCellPassed)
        {
            for (var i = 0; i < numOfCellPassed; i++)
            {
                var finalBlockCell = GetCell(selectedBlock.posX + i, selectedBlock.posY);
                finalBlockCell.cellSituation = cellSituation;
                finalBlockCell.block = null;
            }
        }
        private void UpdateHorizontalBlockStatus(BlockCell selectedBlock, CellSituation cellSituation)
        {
            for (var i = 0; i < selectedBlock.length; i++)
            {
                var finalBlockCell = GetCell(selectedBlock.posX + i, selectedBlock.posY);
                finalBlockCell.cellSituation = cellSituation;
                finalBlockCell.block = null;
            }
        }
        IEnumerator MoveAndDestroyVertically(BlockCell block, ExitGate exitGate)
        {
            gridBuilder.BlockList.Remove(block);

            var targetPosition = new Vector3(block.transform.position.x, block.transform.position.y, exitGate.transform.position.z);
            // Move towards the gate
            block.transform.DOMove(targetPosition, moveDuration).SetEase(Ease.Linear);

            yield return new WaitForSeconds(moveDuration);
            // Perform destruction or slicing effect
            PlayChopEffect(block,exitGate.direction);
        }
        IEnumerator MoveAndDestroyHorizontally(BlockCell block, ExitGate exitGate)
        {
            gridBuilder.BlockList.Remove(block);

            var targetPosition = new Vector3(exitGate.transform.position.x, block.transform.position.y, block.transform.position.z);
            // Move towards the gate
            block.transform.DOMove(targetPosition, moveDuration).SetEase(Ease.Linear);

            yield return new WaitForSeconds(moveDuration);
            // Perform destruction or slicing effect
            PlayChopEffect(block,exitGate.direction);
        }
        void PlayChopEffect(BlockCell block, int direction = 0)
        {
            var particleRotation = GetParticleRotation(direction);

            var particlesInstance = Instantiate(particleSystemPrefab[block.colorValue], block.transform.position, particleRotation);
            // Optionally parent it to the obstacle's parent
            particlesInstance.transform.parent = transform.parent;
            // Play the particle system
            particlesInstance.Play();
            // Subscribe to the particle system's finished event to destroy it after playing
            Destroy(particlesInstance.gameObject, particlesInstance.main.duration);

            // Simulate some delay for the effect
            Destroy(block.gameObject);
        }
        Quaternion GetParticleRotation(int direction)
        {
            switch (direction)
            {
                case 0: // Up
                    return Quaternion.Euler(0, 0, 0);
                case 1: // Right
                    return Quaternion.Euler(0, 90, 0);
                case 2: // Down
                    return Quaternion.Euler(0, 180, 0);
                case 3: // Left
                    return Quaternion.Euler(0, 270, 0);
                default:
                    return Quaternion.Euler(-90, 0, 0); // Default to Up if direction is unknown
            }
        }
        private Cell GetCell(int x, int y)
        {
            return _cellList.Find(exit => exit.posX == x && exit.posY == y);
        }
        private ExitGate GetExitGate(int x, int y, List<int> directions)
        {
            return _exitList.Find(exit => exit.posX == x && exit.posY == y && directions.Contains(exit.direction));
        }
        private void OnEnable()
        {
            GameManager.Instance.OnBlockSwiped += OnBlockSwiped;
        }
        private void OnDisable()
        {
            GameManager.Instance.OnBlockSwiped -= OnBlockSwiped;
        }
    }
}