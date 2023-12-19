using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the flexion angle of the left shoulder.
    /// </summary>
    public class ShoulderLeftFlexion : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShoulderLeftFlexion"/>.
        /// </summary>
        public ShoulderLeftFlexion()
        {
            Type = MeasurementType.ShoulderLeftFlexion;

            KeyJoint1 = JointType.Neck;
            KeyJoint2 = JointType.ShoulderLeft;
            KeyJoint3 = JointType.ElbowLeft;
        }

        public override void Update(Body body)
        {
            Joint neck = body.Joints[KeyJoint1];
            Joint shoulder = body.Joints[KeyJoint2];
            Joint elbow = body.Joints[KeyJoint3];

            Vector3D neck3D = neck.Position3D;
            Vector3D shoulder3D = shoulder.Position3D;
            Vector3D elbow3D = elbow.Position3D;

            float angle = Calculations.Angle(neck3D, shoulder3D, elbow3D);

            _value = angle;
            _angleStart = neck.Position2D;
            _angleCenter = shoulder.Position2D;
            _angleEnd = elbow.Position2D;
        }
    }
}