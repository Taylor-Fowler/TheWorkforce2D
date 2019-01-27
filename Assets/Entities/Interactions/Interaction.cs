using TheWorkforce.Game_State;

namespace TheWorkforce.Entities.Interactions
{
    public abstract class Interaction : TickAction
    {
        /// <summary>
        /// The EntityInstance that is being targeted by this interaction
        /// </summary>
        public readonly EntityInstance Target;

        /// <summary>
        /// Determines whether the interaction requires constant attention
        /// </summary>
        public readonly bool RequiresConstantInteraction;

        public Interaction(EntityInstance target, bool requiresConstantInteraction)
        {
            Target = target;
            RequiresConstantInteraction = requiresConstantInteraction;
            Target.OnEntityDestroy += Destroy;
        }

        /// <summary>
        /// Calls TickAction.Destroy and unsubscribes from any events that was subscribed to
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
            Target.OnEntityDestroy -= Destroy;
        }
    }
}
