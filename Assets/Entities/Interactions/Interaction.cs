using TheWorkforce.Game_State;

namespace TheWorkforce.Entities.Interactions
{
    public delegate void EndOfInteractionHandler();

    public abstract class Interaction : TickAction
    {
        public readonly EntityInstance Target;
        public readonly bool RequiresConstantInteraction;

        // Pass a game timer into the interaction
        public Interaction(EntityInstance target, bool requiresConstantInteraction)
        {
            Target = target;
            RequiresConstantInteraction = requiresConstantInteraction;
            Target.OnEntityDestroy += Destroy;
        }

        public override void Destroy()
        {
            base.Destroy();
            Target.OnEntityDestroy -= Destroy;
        }
    }
}
