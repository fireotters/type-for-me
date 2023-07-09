using GameLogic.Keyboard;
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
        public string LevelName, ScoreType1, ScoreType2;
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
        // no payload needed
    }
    public struct SignalKeyboardEnterPress
    {
        // TODO Implement
    }

    public struct SignalKeyboardPausePress
    {
        // no payload needed
    }

    // --------------------------------------------------------------------------------------------------------------
    // Character Signals
    // --------------------------------------------------------------------------------------------------------------

    public struct SignalArmStopMovement
    {
        public bool iWantToStopArm;
    }
}

