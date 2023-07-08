namespace Signals
{
    // UI Signals
    public struct SignalUiMainMenuStartGame
    {
        public string levelToLoad;
    }

    public struct SignalUiMainMenuTooltipChange
    {
        public bool Showing;
        public string LevelName, ScoreType1, ScoreType2;
    }
    
    // Game End Signals
    public enum GameEndCondition
    {
        Loss, WinType1, WinType2
    }
    public struct SignalGameEnded
    {
        public GameEndCondition result;
        public int score;
    }
    
    // Game Signals
    public struct SignalToggleEffect
    {
        public bool Enabled;
    }

    public struct SignalKeyboardKeyPress
    {
        public string Letter;
    }

    public struct SignalKeyboardBackspacePress
    {
        // no payload needed
    }
}

