using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public int MoveLimit;
    public int RowCount;
    public int ColCount;
    public List<CellInfo> CellInfo;
    public List<MovableInfo> MovableInfo;
    public List<ExitInfo> ExitInfo;
}

[System.Serializable]
public class CellInfo
{
    public int Row;
    public int Col;
}

[System.Serializable]
public class MovableInfo
{
    public int Row;
    public int Col;
    public List<int> Direction;
    public int Length;
    public int Colors;
}

[System.Serializable]
public class ExitInfo
{
    public int Row;
    public int Col;
    public int Direction;
    public int Colors;
}