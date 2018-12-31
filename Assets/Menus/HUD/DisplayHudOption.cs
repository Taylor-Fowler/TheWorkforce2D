using TheWorkforce.UI;

namespace TheWorkforce
{
    public class DisplayHudOption : HudOption
    {
        private IDisplay _display;

        public void SetDisplay(IDisplay display)
        {
            _display = display;
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
    }
}
