using Signals;
using UnityEngine;

namespace GameLogic
{
    public class TestGameManager : MonoBehaviour
    {
        public void TestWin1(int points)
        {
            SignalBus<SignalGameEnded>.Fire(new SignalGameEnded { result = GameEndCondition.WinType1, score = points });
        }

        public void TestWin2(int points)
        {
            SignalBus<SignalGameEnded>.Fire(new SignalGameEnded { result = GameEndCondition.WinType2, score = points });
        }

        public void TestLose()
        {
            SignalBus<SignalGameEnded>.Fire(new SignalGameEnded { result = GameEndCondition.Loss });
        }
    }
}