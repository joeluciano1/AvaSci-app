using System;
using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Represents a measurement of a specific type with a specific value.
    /// </summary>
    public class Measurement
    {
        /// <summary>
        /// The measurement value.
        /// </summary>
        protected float _value = 0.0f;

        /// <summary>
        /// The starting position of the angle segment in the 2D screen space.
        /// </summary>
        protected Vector2D _angleStart = Vector2D.Zero;

        /// <summary>
        /// The ending position of the angle segment in the 2D screen space.
        /// </summary>
        protected Vector2D _angleCenter = Vector2D.Zero;

        /// <summary>
        /// The ending position of the angle segment in the 2D screen space.
        /// </summary>
        protected Vector2D _angleEnd = Vector2D.Zero;

        /// <summary>
        /// The type of the measurement (e.g., elbow flexion, neck rotation).
        /// </summary>
        public MeasurementType Type { get; protected set; } = MeasurementType.None;

        /// <summary>
        /// The first key joint of the measurement segment.
        /// </summary>
        public JointType KeyJoint1 { get; protected set; }

        /// <summary>
        /// The middle/center key joint of the measurement segment.
        /// </summary>
        public JointType KeyJoint2 { get; protected set; }

        /// <summary>
        /// The last key joint of the measurement segment.
        /// </summary>
        public JointType KeyJoint3 { get; protected set; }

        /// <summary>
        /// The numeric value of the measurement (expressed in e.g., degrees or meters).
        /// </summary>
        public float Value => _value;

        /// <summary>
        /// The starting position of the angle segment in the 2D screen space.
        /// </summary>
        public Vector2D AngleStart => _angleStart;

        /// <summary>
        /// The middle/center position of the angle segment in the 2D screen space.
        /// </summary>
        public Vector2D AngleCenter => _angleCenter;

        /// <summary>
        /// The ending position of the angle segment in the 2D screen space.
        /// </summary>
        public Vector2D AngleEnd => _angleEnd;

        /// <summary>
        /// Updates the current movement with the specified skeleton data.
        /// </summary>
        /// <param name="body">The body to check.</param>
        public virtual void Update(Body body)
        {
        }

        /// <summary>
        /// Creates a new instance of the specified measurement.
        /// </summary>
        /// <param name="type">The type of the measurement.</param>
        /// <returns>The joint measurement.</returns>
        public static Measurement Create(MeasurementType type)
        {
            switch (type)
            {
                case MeasurementType.ElbowLeftFlexion:
                    return new ElbowLeftFlexion();
                case MeasurementType.ElbowRightFlexion:
                    return new ElbowRightFlexion();
                case MeasurementType.HipLeftAbduction:
                    return new HipLeftAbduction();
                case MeasurementType.HipLeftFlexion:
                    return new HipLeftFlexion();
                case MeasurementType.HipRightAbduction:
                    return new HipRightAbduction();
                case MeasurementType.HipRightFlexion:
                    return new HipRightFlexion();
                case MeasurementType.KneeLeftFlexion:
                    return new KneeLeftFlexion();
                case MeasurementType.KneeRightFlexion:
                    return new KneeRightFlexion();
                case MeasurementType.NeckLateralFlexion:
                    return new NeckLateralFlexion();
                case MeasurementType.NeckRotation:
                    return new NeckRotation();
                case MeasurementType.ShoulderLeftAbduction:
                    return new ShoulderLeftAbduction();
                case MeasurementType.ShoulderLeftRotation:
                    return new ShoulderLeftRotation();
                case MeasurementType.ShoulderRightAbduction:
                    return new ShoulderRightAbduction();
                case MeasurementType.ShoulderRightRotation:
                    return new ShoulderRightRotation();
                case MeasurementType.ShoulderLeftFlexion:
                    return new ShoulderLeftFlexion();
                case MeasurementType.ShoulderRightFlexion:
                    return new ShoulderRightFlexion();
                case MeasurementType.None:
                    return new Measurement();
                default:
                    throw new NotImplementedException($"The {type} measurement is not implemented!");
            }
        }
    }
}