using UnityEngine;

namespace Objects
{
    public enum CellSituation
    {
        Empty, // Empty tile
        HasMovableBlock // New tile type for blocks
    }

    public class Cell : MonoBehaviour
    {
        public CellSituation cellSituation;
        public BlockCell block;
        public int posX, posY;
        
        
        public void SetCoordinates(int x, int y)
        {
            posX = x;
            posY = y;
            gameObject.name = "Cell: (" + x + ") (" + y + ")";
        }
        public void SetCellSituation(CellSituation cellSituation)
        {
            this.cellSituation = cellSituation;
        }
        
    }
}