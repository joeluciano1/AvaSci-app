using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the flexion angle of the left hip.
    /// </summary>
    public class HipLeftFlexion : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="HipLeftFlexion"/>.
        /// </summary>
        public HipLeftFlexion()
        {
            Type = MeasurementType.HipLeftFlexion;
            
            KeyJoint1 = JointType.HipLeft;
            KeyJoint2 = JointType.KneeLeft;
            KeyJoint3 = JointType.AnkleLeft;
        }

        public override void Update(Body body)
        {
            Joint hip = body.Joints[KeyJoint1];
            Joint knee = body.Joints[KeyJoint2];

            Vector3D hip3D = hip.Position3D;
            Vector3D knee3D = knee.Position3D;

            float angle = 90.0f - Calculations.Rotation(hip3D, knee3D, Plane.Transverse);

            if (knee3D.Y < hip3D.Y)
            {
                angle = 180.0f - angle;
            }

            _value = angle;
            _angleStart = knee.Position2D;
            _angleCenter = hip.Position2D;
            _angleEnd = new Vector2D(hip.Position2D.X, knee.Position2D.Y);
        }
    }
}