using UnityEngine;

namespace GRstory.Character
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] private Light _light;
        [SerializeField] private bool _startsOn = false;

        public bool IsOn { get; private set; }

        #region MonoBehaviour
        private void Awake()
        {
            SetOn(_startsOn);
        }
        #endregion

        public void Toggle()
        {
            SetOn(!IsOn);
        }

        public void SetOn(bool isOn)
        {
            IsOn = isOn;
            if (_light != null) _light.enabled = isOn;
        }
    }
}
