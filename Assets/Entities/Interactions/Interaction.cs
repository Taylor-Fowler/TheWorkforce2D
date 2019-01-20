using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheWorkforce.Game_State;

namespace TheWorkforce.Entities.Interactions
{
    public delegate void EndOfInteractionHandler();

    public abstract class Interaction : IProcessTick
    {
        public readonly EntityInstance Target;
        public readonly bool RequiresConstantInteraction;

        // Pass a game timer into the interaction
        public Interaction(EntityInstance target, bool requiresConstantInteraction)
        {
            Target = target;
            RequiresConstantInteraction = requiresConstantInteraction;
        }        

        public abstract void ProcessTick(float time);
    }
}
