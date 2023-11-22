using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the flexion angle of the left knee.
    /// </summary>
    public class KneeLeftFlexion : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="KneeLeftFlexion"/>.
        /// </summary>
        public KneeLeftFlexion()
        {
            Type = MeasurementType.KneeLeftFlexion;
            
            KeyJoint1 = JointType.HipLeft;
            KeyJoint2 = JointType.KneeLeft;
            KeyJoint3 = JointType.AnkleLeft;
        }

        public override void Update(Body body)
        {
            Joint hip = body.Joints[KeyJoint1];
            Joint knee = body.Joints[KeyJoint2];
            Joint ankle = body.Joints[KeyJoint3];

            Vector3D hip3D = hip.Position3D;
            Vector3D knee3D = knee.Position3D;
            Vector3D ankle3D = ankle.Position3D;

            float angle = 180.0f - Calculations.Angle(hip3D, knee3D, ankle3D);

            _value = angle;
            _angleStart = hip.Position2D;
            _angleCenter = knee.Position2D;
            _angleEnd = ankle.Position2D;
        }
    }
}