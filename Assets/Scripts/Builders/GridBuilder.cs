using System.Collections.Generic;
using Managers;
using Objects;
using UnityEngine;

namespace Builders
{
    public class GridBuilder : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private List<GameObject> blockPrefabs = new List<GameObject>();
        [SerializeField] private GameObject exitGateLeftPrefab;
        [SerializeField] private GameObject exitGateRightPrefab;
        [SerializeField] public GameObject backgroundPlane;
        [SerializeField] private List<BlockSO> blockScriptableObjects;

        private LevelData _currentLevel;
        private Renderer _cellRenderer;
        private Renderer _exitRenderer;
        public float cellHeight;
        public float cellWidth;
        private float _exitHeight;
        private float _exitWidth;
        public float spawnPointXOffset;
        public float spawnPointZOffset;

        private List<Cell> _cellList = new List<Cell>();
        private List<ExitGate> _exitList = new List<ExitGate>();
        private List<BlockCell> _blockList = new List<BlockCell>();

        public List<Cell> CellList => _cellList;
        public List<ExitGate> ExitList => _exitList;
        public List<BlockCell> BlockList => _blockList;

        private void Start()
        {
            InitializeLevelData();
            InitializeDimensions();
            PlaceGridCells();
            PlaceBlocks();
            PlaceExits();
        }

        private void InitializeLevelData()
        {
            _currentLevel = LevelManager.Instance.CurrentLevel;
        }
        private void InitializeDimensions()
        {
            _cellRenderer = cellPrefab.GetComponent<Renderer>();
            _exitRenderer = exitGateLeftPrefab.GetComponent<Renderer>();
            cellHeight = _cellRenderer.bounds.size.z;
            cellWidth = _cellRenderer.bounds.size.x;
            _exitHeight = _exitRenderer.bounds.size.z;
            _exitWidth = _exitRenderer.bounds.size.x;
        }
        private void PlaceGridCells()
        {
            spawnPointXOffset = ((float)_currentLevel.ColCount / 2 * cellWidth) - (cellWidth / 2);
            spawnPointZOffset = ((float)_currentLevel.RowCount / 2 * cellHeight) - (cellHeight / 2);
            foreach (var cellInfo in _currentLevel.CellInfo)
            {
                var position = CalculateCellPosition(cellInfo.Col, cellInfo.Row);
                var cellGameObject = Instantiate(cellPrefab, position, Quaternion.identity, backgroundPlane.transform);
                var cell = cellGameObject.GetComponent<Cell>();
                cell.SetCoordinates(cellInfo.Col, cellInfo.Row);
                _cellList.Add(cell);
            }
        }
        private Vector3 CalculateCellPosition(int col, int row)
        {
            var xPosition = (col * cellWidth) - spawnPointXOffset;
            var zPosition = -(row * cellHeight) + spawnPointZOffset;
            return new Vector3(xPosition, 0.05f, zPosition);
        }
        private void PlaceBlocks()
        {
            foreach (var blockInfo in _currentLevel.MovableInfo)
            {
                var blockNode = CreateBlock(blockInfo);
                var position = CalculateBlockPosition(blockInfo.Col, blockInfo.Row);
                blockNode.transform.position = position;
                _blockList.Add(blockNode);

                SetBlockCells(blockInfo, blockNode);
                blockNode.SetCoordinates(blockInfo.Col, blockInfo.Row);
                RotateBlock(blockInfo, blockNode);
            }
        }
        private BlockCell CreateBlock(MovableInfo blockInfo)
        {
            var prefab = blockPrefabs[blockInfo.Length - 1];
            return new BlockBuilder()
                .SetLength(blockInfo.Length)
                .SetColorValue(blockInfo.Colors)
                .SetDirection(blockInfo.Direction)
                .SetPrefab(prefab)
                .SetScriptableObjects(blockScriptableObjects)
                .Build(backgroundPlane.transform);
        }
        private Vector3 CalculateBlockPosition(int col, int row)
        {
            var xPosition = (col * cellWidth) - spawnPointXOffset;
            var zPosition = -(row * cellHeight) + spawnPointZOffset;
            return new Vector3(xPosition, 0.1f, zPosition);
        }
        private void SetBlockCells(MovableInfo blockInfo, BlockCell blockNode)
        {
            for (int i = 0; i < blockInfo.Length; i++)
            {
                int col = blockInfo.Col;
                int row = blockInfo.Row;

                if (blockInfo.Direction[0] == 0) // Down
                {
                    row += i;
                }
                else if (blockInfo.Direction[0] == 1) // Right
                {
                    col += i;
                }

                var cell = GetCell(col, row);
                if (cell != null)
                {
                    cell.SetCellSituation(CellSituation.HasMovableBlock);
                    cell.block = blockNode;
                }
            }
        }
        private void RotateBlock(MovableInfo blockInfo, BlockCell blockNode)
        {
            if (blockInfo.Direction[0] == 0)
            {
                blockNode.transform.localRotation = Quaternion.Euler(0, 45 * blockInfo.Direction[1], 0);
            }
        }
        private void PlaceExits()
        {
            foreach (var exitInfo in _currentLevel.ExitInfo)
            {
                PlaceExit(exitInfo);
            }
        }
        private void PlaceExit(ExitInfo exitInfo)
        {
            var prefab = exitInfo.Direction == 3 ? exitGateLeftPrefab : exitGateRightPrefab;
            var offset = GetExitOffset(exitInfo.Direction);
            var exitOffset = GetSideExitOffset(exitInfo.Direction);
            var position = CalculateCellPosition(exitInfo.Col, exitInfo.Row);
            var exitPosition = position + offset;
            var exit = InstantiateExit(prefab, exitPosition, exitInfo);
            var exit2Position = exitPosition + exitOffset;
            var exit2 = InstantiateExit(prefab, exit2Position, exitInfo);

            RotateExit(exit, exitInfo.Direction);
            RotateExit(exit2, exitInfo.Direction);
        }
        private GameObject InstantiateExit(GameObject prefab, Vector3 position, ExitInfo exitInfo)
        {
            var exit = Instantiate(prefab, position, Quaternion.identity, backgroundPlane.transform);
            var exitGateComponent = exit.GetComponent<ExitGate>();
            exitGateComponent.SetCoordinates(exitInfo.Col, exitInfo.Row);
            exitGateComponent.Initialize(exitInfo.Colors, exitInfo.Direction);
            _exitList.Add(exitGateComponent);
            return exit;
        }
        private void RotateExit(GameObject exit, int direction)
        {
            exit.transform.localRotation = Quaternion.Euler(0, 90 * direction, 0);
        }
        private Vector3 GetExitOffset(int direction)
        {
            return direction switch
            {
                0 => new Vector3(-_exitWidth / 2, 0, cellHeight / 2 + _exitHeight / 2), // Up
                1 => new Vector3(cellWidth / 2 + _exitHeight / 2, 0, -_exitWidth / 2), // Right
                2 => new Vector3(-_exitWidth / 2, 0, -cellHeight / 2 - _exitHeight / 2), // Down
                3 => new Vector3(-cellWidth / 2 - _exitHeight / 2, 0, _exitWidth / 2), // Left
                _ => Vector3.zero,
            };
        }
        private Vector3 GetSideExitOffset(int direction)
        {
            return direction switch
            {
                0 => new Vector3(_exitWidth, 0, 0), // Up
                1 => new Vector3(0, 0, _exitWidth / 2 + _exitWidth / 2), // Right
                2 => new Vector3(_exitWidth, 0, 0), // Down
                3 => new Vector3(0, 0, -_exitWidth / 2 - _exitWidth / 2), // Left
                _ => Vector3.zero,
            };
        }
        private Cell GetCell(int col, int row)
        {
            return CellList.Find(cell => cell.posX == col && cell.posY == row);
        }
    }
}
