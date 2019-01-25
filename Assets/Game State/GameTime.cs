using System.Collections.Generic;

namespace TheWorkforce.Game_State
{
    public static class GameTime
    {
        public static uint Time = 0;
        private static List<TickAction> _tickActions = new List<TickAction>();

        public static void SubscribeToUpdate(TickAction action)
        {
            _tickActions.Add(action);
        }

        public static void UnsubscribeToUpdate(TickAction action)
        {
            _tickActions.Remove(action);
        }

        public static void Update()
        {
            Time++;
            foreach(var action in _tickActions)
            {
                action.Execute();
            }
        }
    } 
}
