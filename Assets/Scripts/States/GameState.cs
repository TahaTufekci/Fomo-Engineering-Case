using System;

namespace States
{
    [Flags]
    public enum GameState
    {
        None = 0,
        WaitingInput = 1 << 0,
        Win = 1 << 1,          
        Lose = 1 << 2,         
        Pause = 1 << 3,        
        Finish = 1 << 4        
    }
}

