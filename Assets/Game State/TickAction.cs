namespace TheWorkforce.Game_State
{
    public abstract class TickAction
    {
        //
        // Add an ondestroy event that can be subscribed to...
        //
        public event DestroyHandler OnDestroy;

        public TickAction()
        {
            GameTime.SubscribeToUpdate(this);
        }

        public abstract void Execute();

        public virtual void Destroy()
        {
            GameTime.UnsubscribeToUpdate(this);
            OnDestroy?.Invoke();
        }
    }
}
