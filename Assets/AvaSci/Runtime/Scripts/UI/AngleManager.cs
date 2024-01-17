using System.Collections.Generic;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// Manages a list of <see cref="Angle2D"/> visual components.
    /// </summary>
    public class AngleManager : MonoBehaviour
    {
        [SerializeField] private ImageView _imageView;
        [SerializeField] private Angle2D _anglePrefab;

        public readonly Dictionary<MeasurementType, Angle2D> _angles = new Dictionary<MeasurementType, Angle2D>();

        private void OnDestroy()
        {
            Clear();
        }

        /// <summary>
        /// Clears the list of <see cref="Angle2D"/> visual components.
        /// </summary>
        public void Clear()
        {
            foreach (KeyValuePair<MeasurementType, Angle2D> angle in _angles)
            {
                Destroy(angle.Value.gameObject);
            }

            _angles.Clear();
        }

        /// <summary>
        /// Loads the specified <see cref="Movement"/> data to the corresponding <see cref="Angle2D"/> visual components.
        /// </summary>
        /// <param name="body">The <see cref="Body"/> skeleton data.</param>
        /// <param name="movement">A collection of <see cref="Measurement"/> data to load.</param>
        public void Load(Body body, Movement movement)
        {
            if (body == null || movement == null)
            {
                Clear();
                return;
            }

            if (movement.Measurements.Count != _angles.Count)
            {
                Clear();
            }

            foreach (Measurement measurement in movement.MeasurementValues)
            {
                Load(body, measurement);
            }
        }

        /// <summary>
        /// Loads the specified <see cref="Measurement"/> data to the corresponding <see cref="Angle2D"/> visual component.
        /// </summary>
        /// <param name="body">The <see cref="Body"/> skeleton data.</param>
        /// <param name="measurement">The <see cref="Measurement"/> data to load.</param>
        public void Load(Body body, Measurement measurement)
        {
            MeasurementType type = measurement.Type;

            if (!_angles.ContainsKey(type))
            {
                Angle2D go = Instantiate(_anglePrefab, transform);
                go.ImageView = _imageView;

                _angles.Add(type, go);
            }

            if (body.Joints[measurement.KeyJoint1].TrackingState == TrackingState.Inferred ||
                    body.Joints[measurement.KeyJoint2].TrackingState == TrackingState.Inferred ||
                    body.Joints[measurement.KeyJoint3].TrackingState == TrackingState.Inferred)
            {
                _angles[type].gameObject.SetActive(false);
                return;
            }

            if (!_angles[type].gameObject.activeSelf)
            {
                _angles[type].gameObject.SetActive(true);
            }

            _angles[type].Load(body, measurement);

        }
    }
}