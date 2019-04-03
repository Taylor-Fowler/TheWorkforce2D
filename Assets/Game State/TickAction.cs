using System;

namespace TheWorkforce.Game_State
{
    using Interfaces;

    public abstract class TickAction
    {
        //
        // Add an ondestroy event that can be subscribed to...
        //
        public event Action OnDestroy;

        public TickAction()
        {
            GameTime.SubscribeToUpdate(Execute);
        }

        public abstract void Execute();

        public virtual void Destroy()
        {
            GameTime.UnsubscribeToUpdate(Execute);
            OnDestroy?.Invoke();
        }
    }
}
