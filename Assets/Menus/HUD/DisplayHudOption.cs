using UnityEngine;
using UnityEngine.UI;

namespace TheWorkforce
{
    using SOs.References;
    using System.Collections;

    public class DisplayHudOption : HudOption
    {
        [SerializeField] private float _displayAnimationDuration = 0.15f;
        [SerializeField] private Image _borderImage;
        [SerializeField] private IDisplayRef _displayRef;

        private IDisplay _display;
        private float _displayTimer;
        private Coroutine _toggleDisplay;

        public override void Startup(HudMenuOptions hudMenuOptions)
        {
            base.Startup(hudMenuOptions);
            _display = _displayRef.Get();

            if(_display == null)
            {
                _displayRef.ReferenceUpdated += Listen;
            }

            // Initialise the default HUD image
            base.Deactivate();

            _displayTimer = _displayAnimationDuration;
        }

        public override void Activate()
        {
            if(_toggleDisplay != null)
            {
                StopCoroutine(_toggleDisplay);
            }
            _toggleDisplay = StartCoroutine(AnimateDisplay());
        }

        public override void Deactivate()
        {
            if (_toggleDisplay != null)
            {
                StopCoroutine(_toggleDisplay);
            }
            _toggleDisplay = StartCoroutine(AnimateHide());
        }

        private void Listen(IDisplay old, IDisplay newest)
        {
            _display = newest;
            if (_display != null)
            {
                _displayRef.ReferenceUpdated -= Listen;
            }
        }

        private IEnumerator AnimateDisplay()
        {
            while (_displayTimer > 0.0f)
            {
                _displayTimer -= Time.deltaTime;
                _borderImage.fillAmount = Mathf.Max(1.0f - (_displayTimer / _displayAnimationDuration), 0.0f);
                yield return null;
            }

            base.Activate();
            _display.Display();
            _displayTimer = 0.0f;
        }

        private IEnumerator AnimateHide()
        {
            while(_displayTimer < _displayAnimationDuration)
            {
                _displayTimer += Time.deltaTime;
                _borderImage.fillAmount = Mathf.Min(1.0f - (_displayTimer / _displayAnimationDuration), 1.0f);
                yield return null;
            }

            base.Deactivate();
            _display.Hide();
            _displayTimer = _displayAnimationDuration;
        }
    }
}
