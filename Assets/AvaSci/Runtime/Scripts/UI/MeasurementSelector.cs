using LightBuzz.AvaSci.Measurements;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// A visual component that allows the user to select a list of <see cref="MeasurementType"/>s to be displayed.
    /// </summary>
    public class MeasurementSelector : MonoBehaviour
    {
        [SerializeField] private GameObject _list;
        [SerializeField] public Toggle[] _toggles;
        [SerializeField] private TMPro.TMP_Text _label;
        [SerializeField] private Image _arrow;

        private readonly HashSet<MeasurementType> _selectedMeasurements = new HashSet<MeasurementType>();

        private bool _listExpanded = false;

        /// <summary>
        /// Raised when the selection of measurements changes.
        /// </summary>
        public UnityEvent<MeasurementType[]> onMeasurementsChanged = new UnityEvent<MeasurementType[]>();

        private void Awake()
        {
            // Uncomment to load all measurements by deafult.

            // foreach (MeasurementType t in Enum.GetValues(typeof(MeasurementType)))
            // {
            //     if (t == MeasurementType.None) continue;
            //     _selectedMeasurements.Add(t);
            // }

            RaiseEvent();
        }

        /// <summary>
        /// Called when a <see cref="MeasurementToggle"/> is turned on or off.
        /// </summary>
        /// <param name="type">The <see cref="MeasurementType"/> to add or remove from the collection.</param>
        /// <param name="isOn">True to add the <see cref="MeasurementType"/> to the collection; false to remove it.</param>
        public void OnMeasurementSelected(MeasurementType type, bool isOn)
        {
            if (isOn)
                _selectedMeasurements.Add(type);
            else
                _selectedMeasurements.Remove(type);

            RaiseEvent();
        }

        /// <summary>
        /// Called when the button is clicked to expand or collapse the list of measurements.
        /// </summary>
        public void OnButtonClick()
        {
            _listExpanded = !_listExpanded;

            var r = _arrow.transform.rotation;

            _arrow.transform.rotation = Quaternion.Euler(_listExpanded ? 180.0f : 0.0f, r.y, r.z);
            _list.SetActive(_listExpanded);
        }

        /// <summary>
        /// Specifies whether the user can select measurements.
        /// User can select measurements before starting a recording.
        /// </summary>
        /// <param name="value">True to enable selection; false otherwise.</param>
        public void SetEnable(bool value)
        {
            foreach (Toggle toggle in _toggles)
            {
                toggle.interactable = value;
            }
        }

        private void RaiseEvent()
        {
            MeasurementType[] types = new MeasurementType[_selectedMeasurements.Count];

            _selectedMeasurements.CopyTo(types);

            onMeasurementsChanged?.Invoke(types);

            _label.text = $"Selected measurements: {_selectedMeasurements.Count}";
        }
    }
}