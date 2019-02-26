using System;
using System.Collections.Generic;

namespace TheWorkforce.Game_State
{
    public static class GameTime
    {
        public static uint Time = 0;
        private static List<TickAction> _tickActions = new List<TickAction>();
        private static List<TickAction> _tickActionsToRemove = new List<TickAction>();

        private static List<Action> _postTickActions = new List<Action>();
        private static List<Action> _postTickActionsToRemove = new List<Action>();

        public static void SubscribeToUpdate(TickAction action) => _tickActions.Add(action);
        public static void UnsubscribeToUpdate(TickAction action) => _tickActionsToRemove.Add(action);

        public static void SubscribeToPostUpdate(Action action) => _postTickActions.Add(action);
        public static void UnsubscribeToPostUpdate(Action action) => _postTickActionsToRemove.Add(action);

        public static void Update()
        {
            Time++;
            foreach(var action in _tickActions)
            {
                action.Execute();
            }

            foreach(var action in _tickActionsToRemove)
            {
                _tickActions.Remove(action);
            }
            _tickActionsToRemove.Clear();
        }

        public static void PostUpdate()
        {
            foreach (var action in _postTickActions)
            {
                action();
            }
            foreach (var action in _postTickActionsToRemove)
            {
                _postTickActions.Remove(action);
            }
            _postTickActionsToRemove.Clear();
        }
    } 
}
