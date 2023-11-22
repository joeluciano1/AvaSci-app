using LightBuzz.AvaSci.Measurements;
using UnityEngine;
using UnityEngine.Events;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// A toggle that can be used to enable/disable a measurement.
    /// </summary>
    public class MeasurementToggle : MonoBehaviour
    {
        [SerializeField] private MeasurementType _type;

        /// <summary>
        /// The <see cref="MeasurementType"/> represented by this toggle.
        /// </summary>
        public MeasurementType MeasurementType
        {
            get => _type;
            set => _type = value;
        }

        /// <summary>
        /// Raised when the toggle is turned on or off.
        /// </summary>
        public UnityEvent<MeasurementType, bool> onMeasurementChanged = new UnityEvent<MeasurementType, bool>();

        /// <summary>
        /// Called when the toggle is turned on or off.
        /// </summary>
        /// <param name="isOn">Specifies whether the toggle is on (true) or off (false).</param>
        public void OnToggle(bool isOn)
        {
            onMeasurementChanged.Invoke(_type, isOn);
        }
    }
}