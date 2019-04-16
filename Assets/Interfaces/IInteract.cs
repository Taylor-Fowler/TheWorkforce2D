using TheWorkforce.Entities;
using TheWorkforce.Entities.Interactions;

namespace TheWorkforce.Interfaces
{
    public interface IInteract : IEntityInteract, IPlayerInteract
    {
    }

    public interface IEntityInteract
    {
        Interaction Interact(EntityInstance initiator);
    }

    public interface IPlayerInteract
    {
        Interaction Interact(Player initiator);
    }
}
