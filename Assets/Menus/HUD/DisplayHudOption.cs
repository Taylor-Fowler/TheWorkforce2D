using UnityEngine;

namespace TheWorkforce
{
    using SOs.References;

    public class DisplayHudOption : HudOption
    {
        [SerializeField] private IDisplayRef _displayRef;
        private IDisplay _display;

        public override void Startup(HudMenuOptions hudMenuOptions)
        {
            base.Startup(hudMenuOptions);
            _display = _displayRef.Get();

            if(_display == null)
            {
                _displayRef.ReferenceUpdated += Listen;
            }
        }

        public override void Activate()
        {
            base.Activate();
            _display.Display();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            _display.Hide();
        }

        private void Listen(IDisplay old, IDisplay newest)
        {
            _display = newest;
            if (_display != null)
            {
                _displayRef.ReferenceUpdated -= Listen;
            }
        }
    }
}
