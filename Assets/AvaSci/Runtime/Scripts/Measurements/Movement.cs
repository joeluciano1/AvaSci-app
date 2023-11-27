using LightBuzz.BodyTracking;
using System.Collections.Generic;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Represents a collection of <see cref="Measurement"/>s.
    /// </summary>
    public class Movement
    {
        private readonly Dictionary<MeasurementType, Measurement> _measurements = new Dictionary<MeasurementType, Measurement>();

        /// <summary>
        /// Returns the <see cref="Measurement"/>s of the current <see cref="Movement"/>.
        /// </summary>
        public Dictionary<MeasurementType, Measurement> Measurements => _measurements;

        /// <summary>
        /// Returns the <see cref="MeasurementType"/>s of the current <see cref="Movement"/>.
        /// </summary>
        public List<MeasurementType> MeasurementTypes => new List<MeasurementType>(_measurements.Keys);

        /// <summary>
        /// Returns the <see cref="Measurement"/> values of the current <see cref="Movement"/>.
        /// </summary>
        public List<Measurement> MeasurementValues => new List<Measurement>(_measurements.Values);

        /// <summary>
        /// Clears and resets the <see cref="Measurement"/>s of the current <see cref="Movement"/>. 
        /// </summary>
        /// <param name="types">The <see cref="MeasurementType"/>s to include.</param>
        public void Reset(params MeasurementType[] types)
        {
            _measurements.Clear();
            ReferenceManager.instance.ClearGraphs();
            foreach (MeasurementType type in types)
            {
                _measurements.Add(type, Measurement.Create(type));
                ReferenceManager.instance.GenerateGraph(type);
            }
        }

        /// <summary>
        /// Updates the <see cref="Measurement"/>s of the current <see cref="Movement"/> with the specified <see cref="Body"/> data.
        /// </summary>
        /// <param name="body">The <see cref="Body"/> skeleton data.</param>
        public void Update(Body body)
        {
            foreach (Measurement measurement in _measurements.Values)
            {
                measurement.Update(body);
            }
        }

        /// <summary>
        /// Returns the <see cref="Measurement"/> of the specified <see cref="MeasurementType"/>.
        /// </summary>
        /// <param name="type">The <see cref="MeasurementType"/> to retrieve from the collection.</param>
        /// <returns>The retrieved <see cref="Measurement"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the specified <see cref="MeasurementType"/> was not found in the collection.</exception>
        public Measurement Get(MeasurementType type)
        {
            if (!_measurements.ContainsKey(type))
            {
                throw new KeyNotFoundException($"Measurement of type {type} not found.");
            }

            return _measurements[type];
        }

        /// <summary>
        /// Returns a string representation of the current collection.
        /// </summary>
        /// <returns>A string representation of the <see cref="Measurement"/>s.</returns>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            
            foreach (var m in Measurements)
            {
                sb.AppendLine($"{m.Key}: {m.Value.Value:N0}");
            }
            
            return sb.ToString();
        }
    }
}