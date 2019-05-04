using System;
using System.Collections.Generic;

namespace TheWorkforce.Game_State
{
    public static class GameTime
    {
        public static uint Time { get; private set; }
        public static uint BackgroundTime { get; private set; }

        private static List<Action> _tickActions = new List<Action>();
        private static List<Action> _tickActionsToRemove = new List<Action>();

        private static List<Action> _postTickActions = new List<Action>();
        private static List<Action> _postTickActionsToRemove = new List<Action>();

        private static Dictionary<uint, List<Action>> _specificTickActions = new Dictionary<uint, List<Action>>();
        private static Dictionary<uint, List<Action>> _specificPostTickActions = new Dictionary<uint, List<Action>>();

        private static Dictionary<uint, List<Action>> _specificBackgroundTickActions = new Dictionary<uint, List<Action>>();

        public static void SubscribeToUpdate(Action action) => _tickActions.Add(action);
        public static void UnsubscribeToUpdate(Action action) => _tickActionsToRemove.Add(action);

        public static void SubscribeToPostUpdate(Action action) => _postTickActions.Add(action);
        public static void UnsubscribeToPostUpdate(Action action) => _postTickActionsToRemove.Add(action);

        public static void ListenForSpecificTick(uint tick, Action action)
        {
            // The tick to listen for must be before the current tick
            if(tick < Time)
            {
                return;
            }

            List<Action> actions;

            // Try to find a list of actions already designated for the specified tick
            // If none are found, create a new list and add it to the dictionary
            if(!_specificTickActions.TryGetValue(tick, out actions))
            {
                actions = new List<Action>();
                _specificTickActions.Add(tick, actions);
            }
            actions.Add(action);
        }

        public static void ListenForSpecificPostTick(uint tick, Action action)
        {
            // The tick to listen for must be before the current tick
            if (tick < Time)
            {
                return;
            }

            List<Action> actions;

            // Try to find a list of actions already designated for the specified tick
            // If none are found, create a new list and add it to the dictionary
            if (!_specificPostTickActions.TryGetValue(tick, out actions))
            {
                actions = new List<Action>();
                _specificPostTickActions.Add(tick, actions);
            }
            actions.Add(action);
        }

        public static void ListenForBackgroundTick(uint ticksToWait, Action action) => ListenForSpecificBackgroundTick(BackgroundTime + ticksToWait, action);

        public static void ListenForSpecificBackgroundTick(uint tick, Action action)
        {
            if(tick < BackgroundTime)
            {
                return;
            }

            List<Action> actions;

            // Try to find a list of actions already designated for the specified tick
            // If none are found, create a new list and add it to the dictionary
            if (!_specificBackgroundTickActions.TryGetValue(tick, out actions))
            {
                actions = new List<Action>();
                _specificBackgroundTickActions.Add(tick, actions);
            }
            actions.Add(action);
        }


        // TODO: Add listen for tick in x ticks



        public static void Update()
        {
            ++Time;

            // Remove any tick actions that have been designated for removal
            foreach (var action in _tickActionsToRemove)
            {
                _tickActions.Remove(action);
            }
            _tickActionsToRemove.Clear();

            // Process the current tick actions
            foreach(var action in _tickActions)
            {
                action.Invoke();
            }

            List<Action> actions;
            if (_specificTickActions.TryGetValue(Time, out actions))
            {
                foreach (var action in actions)
                {
                    action.Invoke();
                }
                _specificTickActions.Remove(Time);
            }

        }

        public static void PostUpdate()
        {
            foreach (var action in _postTickActionsToRemove)
            {
                _postTickActions.Remove(action);
            }
            _postTickActionsToRemove.Clear();

            foreach (var action in _postTickActions)
            {
                action();
            }

            List<Action> actions;
            if (_specificPostTickActions.TryGetValue(Time, out actions))
            {
                foreach (var action in actions)
                {
                    action.Invoke();
                }
                _specificPostTickActions.Remove(Time);
            }
        }

        public static void UpdateBackgroundTimer()
        {
            ++BackgroundTime;

            List<Action> actions;
            if (_specificBackgroundTickActions.TryGetValue(BackgroundTime, out actions))
            {
                foreach (var action in actions)
                {
                    action.Invoke();
                }
                _specificBackgroundTickActions.Remove(BackgroundTime);
            }
        }

        public static void Reset()
        {
            _tickActions.Clear();
            _tickActionsToRemove.Clear();
            _specificTickActions.Clear();

            _specificBackgroundTickActions.Clear();

            _postTickActions.Clear();
            _postTickActionsToRemove.Clear();
            _specificPostTickActions.Clear();

        }
    } 
}
