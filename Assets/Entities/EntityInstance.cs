using TheWorkforce.UI;

namespace TheWorkforce.Entities
{
    public abstract class EntityInstance : IDisplay
    {
        private uint _id;

        public EntityInstance(uint id)
        {
            _id = id;
        }

        public uint GetId()
        {
            return _id;
        }

        public abstract void Display();
        public abstract void Hide();
    }
}
