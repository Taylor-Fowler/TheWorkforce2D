using TheWorkforce.Entities;
using TheWorkforce.Entities.Interactions;

namespace TheWorkforce.Interfaces
{
    public interface IInteract
    {
        Interaction Interact(EntityInstance initiator);
    }
}
