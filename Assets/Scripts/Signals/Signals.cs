using System.Numerics;

namespace Signals
{
    // --------------------------------------------------------------------------------------------------------------
    // UI Signals
    // --------------------------------------------------------------------------------------------------------------
    public struct SignalUiMainMenuStartGame
    {
        public string levelToLoad;
    }

    public struct SignalUiMainMenuTooltipChange
    {
        public bool Showing;
        public string LevelName, Acc, Com;
    }

    // --------------------------------------------------------------------------------------------------------------
    // Game End Functions
    // --------------------------------------------------------------------------------------------------------------
    public enum GameEndCondition
    {
        Loss, Win
    }
    public struct SignalGameEnded
    {
        public GameEndCondition result;
        public int bestCombo;
        public int accuracy;
        public int levelHighestComboPossible;
    }

    // --------------------------------------------------------------------------------------------------------------
    // Key Signals
    // --------------------------------------------------------------------------------------------------------------
    public struct SignalKeyboardKeyPress
    {
        public string Letter;
    }

    public struct SignalKeyboardBackspacePress
    {
        // no payload
    }

    public struct SignalKeyboardPausePress
    {
        // no payload
    }

    // --------------------------------------------------------------------------------------------------------------
    // Character Signals
    // --------------------------------------------------------------------------------------------------------------

    public struct SignalArmStopMovement
    {
        public bool iWantToStopArm;
    }
}

