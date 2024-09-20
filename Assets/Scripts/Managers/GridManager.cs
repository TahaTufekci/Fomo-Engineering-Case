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
        // The class manages the level data and the movement of blocks in the grid.
        private LevelData _currentLevel;
        private List<Cell> _cellList = new List<Cell>(); // List to store all cells in the grid
        private List<ExitGate> _exitList = new List<ExitGate>(); // List to store exit gates

        [SerializeField] private List<ParticleSystem> particleSystemPrefab; // List to store particle system prefabs for effects
        [SerializeField] private GridBuilder gridBuilder; // Reference to the GridBuilder component

        public float moveDuration = 0.3f; // Duration of the block movement animation

        // Start method is called before the first frame update. Initializes level data and retrieves the cell and exit lists.
        private void Start()
        {
            _currentLevel = LevelManager.Instance.CurrentLevel; // Get current level from LevelManager
            _cellList = gridBuilder.CellList; // Get the list of cells from GridBuilder
            _exitList = gridBuilder.ExitList; // Get the list of exit gates from GridBuilder
        }

        /// <summary>
        /// Handles block swipe events and determines if the block can move in the swipe direction.
        /// </summary>
        /// <param name="selectedBlock">The block being swiped.</param>
        /// <param name="swipeDirection">The direction of the swipe.</param>
        private void OnBlockSwiped(BlockCell selectedBlock, Vector2 swipeDirection)
        {
            if (selectedBlock == null) return; // Return if no block is selected

            // Map the swipe direction to a specific direction (e.g. up, down, left, right)
            var direction = DirectionMapper.GetDirection(swipeDirection);

            // If the selected block doesn't have the ability to move in the swipe direction, log the issue and return.
            if (!selectedBlock.directions.Contains(direction))
            {
                Debug.Log("Block cannot move in the swipe direction");
                return;
            }

            // Handle block movement in the swipe direction
            HandleMovement(selectedBlock, swipeDirection, direction);
        }

        /// <summary>
        /// Handles the movement logic for the selected block based on the swipe direction.
        /// </summary>
        /// <param name="selectedBlock">The block to move.</param>
        /// <param name="swipeDirection">The direction of the swipe.</param>
        /// <param name="direction">The mapped direction (e.g. up, down, left, right).</param>
        private void HandleMovement(BlockCell selectedBlock, Vector2 swipeDirection, int direction)
        {
            // If direction is horizontal (1 = right, 3 = left)
            if (direction is 1 or 3)
            {
                HandleHorizontalMovement(selectedBlock, swipeDirection); // Handle horizontal movement
            }
            else // For vertical direction (0 = up, 2 = down)
            {
                HandleVerticalMovement(selectedBlock, swipeDirection); // Handle vertical movement
            }
        }

        /// <summary>
        /// Moves the block vertically, either up or down based on the swipe direction.
        /// </summary>
        /// <param name="selectedBlock">The block to move.</param>
        /// <param name="swipeDirection">The vertical direction (up or down).</param>
        private void HandleVerticalMovement(BlockCell selectedBlock, Vector2 swipeDirection)
        {
            if (swipeDirection == Vector2.down) // Move down
                MoveBlock(selectedBlock, swipeDirection);
            else // Move up
                MoveBlock(selectedBlock, swipeDirection);
        }

        /// <summary>
        /// Moves the block horizontally, either left or right based on the swipe direction.
        /// </summary>
        /// <param name="selectedBlock">The block to move.</param>
        /// <param name="swipeDirection">The horizontal direction (left or right).</param>
        private void HandleHorizontalMovement(BlockCell selectedBlock, Vector2 swipeDirection)
        {
            if (swipeDirection == Vector2.right) // Move right
                MoveBlock(selectedBlock, swipeDirection);
            else // Move left
                MoveBlock(selectedBlock, swipeDirection);
        }

        /// <summary>
        /// Moves the block in the given direction after verifying the movement is valid.
        /// </summary>
        /// <param name="selectedBlock">The block to move.</param>
        /// <param name="direction">The direction to move the block in.</param>
        private void MoveBlock(BlockCell selectedBlock, Vector2 direction)
        {
            int checkIndex;

            // Determine the index based on whether the movement is vertical or horizontal
            if (direction == Vector2.up || direction == Vector2.down)
                checkIndex = GetVerticalCheckIndex(selectedBlock, direction); // Get vertical index for checking
            else
                checkIndex = GetHorizontalCheckIndex(selectedBlock, direction); // Get horizontal index for checking

            // Calculate the number of cells the block will pass through
            int updatedCheckIndex;
            int numOfCellPassed = CalculatePassedCells(selectedBlock, checkIndex, direction, out updatedCheckIndex);
            checkIndex = updatedCheckIndex;

            // If the next cell is valid and has a movable block, proceed with the move
            if (IsValidCell(checkIndex, direction) && GetTargetCell(selectedBlock, checkIndex, direction).cellSituation == CellSituation.HasMovableBlock)
            {
                if (numOfCellPassed == 0) return; // If no cells passed, do nothing

                // Update the cells based on how many cells were passed
                if (numOfCellPassed >= selectedBlock.length)
                {
                    UpdateCells(selectedBlock, CellSituation.Empty, direction);
                }
                else
                {
                    UpdateCells(selectedBlock, CellSituation.Empty, direction, numOfCellPassed);
                }

                // If the block has more than 1 length, adjust the index for movement
                if (selectedBlock.length > 1)
                {
                    checkIndex -= selectedBlock.length - 1;
                }

                // Move the block in the specified direction
                MoveBlockInDirection(selectedBlock, direction, checkIndex);

                // Update the cells to reflect the block's new position
                UpdateCells(selectedBlock, CellSituation.HasMovableBlock, direction);
            }
            else
            {
                // If the block reaches an exit, handle exit movement logic
                HandleExitMovement(selectedBlock, direction, checkIndex, numOfCellPassed);
            }
        }

        /// <summary>
        /// Gets the vertical check index for determining the block's position.
        /// </summary>
        /// <returns>The index of the block's vertical position.</returns>
        private int GetVerticalCheckIndex(BlockCell selectedBlock, Vector2 direction)
        {
            return direction == Vector2.up ? selectedBlock.posY - 1 : selectedBlock.posY + selectedBlock.length;
        }

        /// <summary>
        /// Returns the horizontal check index for the block movement based on the direction.
        /// </summary>
        /// <returns>Check index for horizontal movement.</returns>
        private int GetHorizontalCheckIndex(BlockCell selectedBlock, Vector2 direction)
        {
            // Determine the check index based on left or right movement
            return direction == Vector2.left ? selectedBlock.posX - 1 : selectedBlock.posX + selectedBlock.length;
        }

        /// <summary>
        /// Calculates the number of passed empty cells in the given direction.
        /// </summary>
        /// <param name="selectedBlock">The block to move.</param>
        /// <param name="checkIndex">The initial check index.</param>
        /// <param name="direction">The direction of movement.</param>
        /// <param name="updatedCheckIndex">The updated check index after calculating passed cells.</param>
        /// <returns>The number of empty cells passed.</returns>
        private int CalculatePassedCells(BlockCell selectedBlock, int checkIndex, Vector2 direction, out int updatedCheckIndex)
        {
            int numOfCells = 0;
            updatedCheckIndex = checkIndex; // Initialize updated check index

            // Continue checking while valid and cell is empty
            while (IsValidCell(updatedCheckIndex, direction) && GetTargetCell(selectedBlock, updatedCheckIndex, direction).cellSituation == CellSituation.Empty)
            {
                numOfCells++;
                updatedCheckIndex = UpdateCheckIndex(updatedCheckIndex, direction);
            }
            return numOfCells;
        }

        /// <summary>
        /// Checks if the cell index is valid based on the direction.
        /// </summary>
        /// <param name="checkIndex">The index to check.</param>
        /// <param name="direction">The direction of movement.</param>
        /// <returns>True if the cell index is valid, false otherwise.</returns>
        private bool IsValidCell(int checkIndex, Vector2 direction)
        {
            // Check validity based on direction (vertical or horizontal)
            return direction == Vector2.up || direction == Vector2.down
                ? checkIndex >= 0 && checkIndex < _currentLevel.RowCount
                : checkIndex >= 0 && checkIndex < _currentLevel.ColCount;
        }

        private int UpdateCheckIndex(int checkIndex, Vector2 direction)
        {
            // Update the check index for upward/left or downward/right movement
            return direction == Vector2.up || direction == Vector2.left ? checkIndex - 1 : checkIndex + 1;
        }

        /// <summary>
        /// Checks if the current cell is an exit gate for the block.
        /// </summary>
        /// <returns>True if the cell is an exit gate, false otherwise.</returns>
        private bool IsExitGate(BlockCell selectedBlock, Vector2 direction, int checkIndex)
        {
            // Determine if the block is at the exit gate based on the direction
            if (direction == Vector2.up || direction == Vector2.left)
                return checkIndex <= 0;
            else
                return checkIndex >= _currentLevel.RowCount - 1 || checkIndex >= _currentLevel.ColCount - 1;
        }

        /// <summary>
        /// Handles the block's movement when it reaches an exit gate.
        /// </summary>
        /// <param name="selectedBlock">The block being moved.</param>
        /// <param name="direction">The direction of movement.</param>
        /// <param name="checkIndex">The current check index.</param>
        /// <param name="numOfCellPassed">The number of cells passed.</param>
        private void HandleExitMovement(BlockCell selectedBlock, Vector2 direction, int checkIndex, int numOfCellPassed)
        {
            ExitGate exitGate = null;

            // Find the correct exit gate based on the movement direction
            if (direction == Vector2.up)
            {
                exitGate = GetExitGate(selectedBlock.posX, 0, selectedBlock.directions);
            }
            else if (direction == Vector2.down)
            {
                exitGate = GetExitGate(selectedBlock.posX, _currentLevel.RowCount - 1, selectedBlock.directions);
            }
            else if (direction == Vector2.left)
            {
                exitGate = GetExitGate(0, selectedBlock.posY, selectedBlock.directions);
            }
            else if (direction == Vector2.right)
            {
                exitGate = GetExitGate(_currentLevel.ColCount - 1, selectedBlock.posY, selectedBlock.directions);
            }

            // If exit gate color matches the block color, destroy the block
            if (exitGate != null && exitGate.colorValue == selectedBlock.colorValue)
            {
                UpdateCells(selectedBlock, CellSituation.Empty, direction);
                if (direction == Vector2.up || direction == Vector2.down)
                {
                    StartCoroutine(MoveAndDestroy(selectedBlock, exitGate, true));
                }
                else
                {
                    StartCoroutine(MoveAndDestroy(selectedBlock, exitGate, false));
                }
                GameManager.Instance.OnValidMove?.Invoke();
            }
            else
            {
                // If no exit, update the cell situation and move the block
                if (numOfCellPassed == 0) return;
                UpdateCells(selectedBlock, CellSituation.Empty, direction);
                MoveBlockInDirection(selectedBlock, direction, checkIndex);
                UpdateCells(selectedBlock, CellSituation.HasMovableBlock, direction);
            }
        }

        /// <summary>
        /// Moves the block to the exit gate and destroys it.
        /// </summary>
        /// <param name="block">The block to move and destroy.</param>
        /// <param name="exitGate">The exit gate for the block.</param>
        /// <param name="isVertical">True if the movement is vertical, false if horizontal.</param>
        /// <returns>Coroutine for moving and destroying the block.</returns>
        IEnumerator MoveAndDestroy(BlockCell block, ExitGate exitGate, bool isVertical)
        {
            gridBuilder.BlockList.Remove(block);
            float adjustment;
            var cubeSize = block.GetComponent<Renderer>().bounds.size;

            // Adjust position for blocks longer than one unit
            if (block.length > 1)
            {
                adjustment = isVertical
                    ? (exitGate.direction == 0)
                        ? -(cubeSize.z / block.length / 2)
                        : (cubeSize.z / block.length / 2) * (block.length * 2) - 0.25f
                    : (exitGate.direction == 1)
                        ? -(cubeSize.x / block.length / 2) * (block.length * 2) - 0.5f
                        : (cubeSize.x / block.length / 2);
            }
            else
            {
                adjustment = isVertical
                    ? (exitGate.direction == 0)
                        ? -cubeSize.z / 2
                        : cubeSize.z / 2
                    : (exitGate.direction == 1)
                        ? -cubeSize.x / 2
                        : cubeSize.x / 2;
            }

            // Set target position for the block to move towards the exit gate
            Vector3 targetPosition = isVertical
                ? new Vector3(block.transform.position.x, block.transform.position.y, exitGate.transform.position.z + adjustment)
                : new Vector3(exitGate.transform.position.x + adjustment, block.transform.position.y, block.transform.position.z);

            // Move the block to the target position
            block.transform.DOMove(targetPosition, moveDuration).SetEase(Ease.Linear);

            yield return new WaitForSeconds(moveDuration);

            // Play destruction or slicing effect after reaching the exit
            PlayChopEffect(block, exitGate.direction);
        }

        /// <summary>
        /// Moves the selected block in the given direction and updates its coordinates.
        /// </summary>
        /// <param name="selectedBlock">The block that is being moved.</param>
        /// <param name="direction">The direction in which the block will move (up, down, left, right).</param>
        /// <param name="checkIndex">The index used to find the target cell in the grid.</param>
        private void MoveBlockInDirection(BlockCell selectedBlock, Vector2 direction, int checkIndex)
        {
            Cell targetCell;

            // Check the direction and update the checkIndex to get the target cell accordingly
            if (direction == Vector2.left)
            {
                targetCell = GetCell(++checkIndex, selectedBlock.posY);  // Moving left
            }
            else if (direction == Vector2.right)
            {
                targetCell = GetCell(--checkIndex, selectedBlock.posY);  // Moving right
            }
            else if (direction == Vector2.up)
            {
                targetCell = GetCell(selectedBlock.posX, ++checkIndex);  // Moving up
            }
            else
            {
                targetCell = GetCell(selectedBlock.posX, --checkIndex);  // Moving down
            }

            // Find the final target cell
            targetCell = GetTargetCell(selectedBlock, checkIndex, direction);

            // Move the block to the target cell's position
            selectedBlock.MoveToTarget(targetCell.transform.position);

            // Update the block's coordinates to match the target cell's
            selectedBlock.SetCoordinates(targetCell.posX, targetCell.posY);

            // Invoke the OnValidMove event if defined
            GameManager.Instance.OnValidMove?.Invoke();
        }

        /// <summary>
        /// Updates the status of the cells occupied by the block.
        /// </summary>
        /// <param name="selectedBlock">The block whose occupied cells will be updated.</param>
        /// <param name="status">The new status to assign to the cells.</param>
        /// <param name="direction">The direction in which the cells are being updated.</param>
        /// <param name="numOfCells">The number of cells to update.</param>
        private void UpdateBlockStatus(BlockCell selectedBlock, CellSituation status, Vector2 direction, int numOfCells)
        {
            for (var i = 0; i < numOfCells; i++)
            {
                // Check direction and get the appropriate cell for updating its status
                Cell currentCell = direction == Vector2.up || direction == Vector2.left
                    ? GetCell(selectedBlock.posX + i, selectedBlock.posY)  // Horizontal movement
                    : GetCell(selectedBlock.posX, selectedBlock.posY + i);  // Vertical movement

                // Update the cell's status and clear its block reference
                currentCell.cellSituation = status;
                currentCell.block = null;
            }
        }

        /// <summary>
        /// Updates the status and block reference of cells based on the direction of movement.
        /// </summary>
        /// <param name="selectedBlock">The block whose associated cells will be updated.</param>
        /// <param name="cellSituation">The new situation to assign to the cells.</param>
        /// <param name="direction">The direction of the block's movement (left, right, up, down).</param>
        /// <param name="numOfCells">Optional parameter for number of cells to update. Defaults to the block's length.</param>
        private void UpdateCells(BlockCell selectedBlock, CellSituation cellSituation, Vector2 direction, int numOfCells = -1)
        {
            // Determine if the movement is horizontal
            var isHorizontal = direction == Vector2.right || direction == Vector2.left ? true : false;

            // If the number of cells isn't provided, use the block's length
            int cellsToUpdate = numOfCells == -1 ? selectedBlock.length : numOfCells;

            // Update the status of the cells affected by the movement
            for (var i = 0; i < cellsToUpdate; i++)
            {
                var posX = isHorizontal ? selectedBlock.posX + i : selectedBlock.posX;
                var posY = isHorizontal ? selectedBlock.posY : selectedBlock.posY + i;

                // Get the cell to be updated and apply the new status and clear the block reference
                var finalBlockCell = GetCell(posX, posY);
                finalBlockCell.cellSituation = cellSituation;
                finalBlockCell.block = null;
            }
        }

        /// <summary>
        /// Plays a particle effect when a block is chopped or destroyed, and starts the shrinking effect.
        /// </summary>
        /// <param name="block">The block that is being chopped.</param>
        /// <param name="direction">The direction of the chop effect. Default is 0.</param>
        void PlayChopEffect(BlockCell block, int direction = 0)
        {
            // Get the rotation for the particle effect based on the direction of movement
            var particleRotation = GetParticleRotation(direction);

            // Instantiate the particle effect at the block's position with the appropriate rotation
            var particlesInstance = Instantiate(particleSystemPrefab[block.colorValue], block.transform.position, particleRotation);

            // Optionally parent the particle system to the block's parent
            particlesInstance.transform.parent = transform.parent;

            // Play the particle system
            particlesInstance.Play();

            // Destroy the particle system after it finishes playing
            Destroy(particlesInstance.gameObject, particlesInstance.main.duration);

            // Start the shrinking and destroying effect on the block
            StartCoroutine(ShrinkAndDestroy(block, direction));
        }

        /// <summary>
        /// Shrinks and then destroys the block over a period of time.
        /// </summary>
        /// <param name="block">The block to be shrunk and destroyed.</param>
        /// <param name="direction">The direction of the shrinking effect.</param>
        /// <returns>IEnumerator for coroutine execution.</returns>
        IEnumerator ShrinkAndDestroy(BlockCell block, int direction)
        {
            float shrinkDuration = 1.0f;  // Duration for shrinking effect
            var originalScale = block.transform.localScale;
            var targetScale = originalScale;

            var originalPosition = block.transform.position;
            var targetPosition = originalPosition;

            // Adjust scale and position based on the direction of movement (left, right, up, down)
            if (direction == 1 || direction == 3) // Right or left
            {
                targetScale = new Vector3(0, originalScale.y, originalScale.z); // Shrink along X axis

                if (direction == 1) // Shrink from the right
                {
                    targetPosition = new Vector3(originalPosition.x + originalScale.x / 2, originalPosition.y, originalPosition.z);
                }
                else // Shrink from the left
                {
                    targetPosition = new Vector3(originalPosition.x - originalScale.x / 2, originalPosition.y, originalPosition.z);
                }
            }
            else if (direction == 0 || direction == 2) // Up or down
            {
                targetScale = new Vector3(0, originalScale.y, originalScale.z); // Shrink along X axis

                if (direction == 0) // Shrink upwards
                {
                    targetPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z + originalScale.z / 2);
                }
                else // Shrink downwards
                {
                    if (block.length > 1)
                    {
                        targetPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z - originalScale.z * 2);
                    }
                    else
                    {
                        targetPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z - originalScale.z / 2);
                    }
                }
            }

            // Perform the shrinking animation
            for (float t = 0; t < shrinkDuration; t += Time.deltaTime)
            {
                // Move the block towards the target position
                block.transform.position = Vector3.Lerp(originalPosition, targetPosition, t / shrinkDuration);
                // Shrink the block towards the target scale
                block.transform.localScale = Vector3.Lerp(originalScale, targetScale, t / shrinkDuration);
                yield return null;
            }

            // Finally destroy the block after shrinking
            Destroy(block.gameObject);
        }
        IEnumerator FadeOut(BlockCell block)
        {
            var blockRenderer = block.GetComponent<Renderer>();
            var blockColor = blockRenderer.material.color;
            for (float t = 0.01f; t < 1; t += Time.deltaTime)
            {
                blockRenderer.material.color = new Color(blockColor.r, blockColor.g, blockColor.b, Mathf.Lerp(1, 0, Mathf.Min(1, t / 1)));
                yield return null;
            }

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
        private Cell GetTargetCell(BlockCell selectedBlock, int checkIndex, Vector2 direction)
        {
            // Return the appropriate cell depending on vertical or horizontal movement
            return direction == Vector2.up || direction == Vector2.down
                ? GetCell(selectedBlock.posX, checkIndex)
                : GetCell(checkIndex, selectedBlock.posY);
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