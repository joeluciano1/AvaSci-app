using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the flexion angle of the left elbow.
    /// </summary>
    public class ElbowLeftFlexion : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="ElbowLeftFlexion"/>.
        /// </summary>
        public ElbowLeftFlexion()
        {
            Type = MeasurementType.ElbowLeftFlexion;
            
            KeyJoint1 = JointType.ShoulderLeft;
            KeyJoint2 = JointType.ElbowLeft;
            KeyJoint3 = JointType.WristLeft;
        }

        public override void Update(Body body)
        {
            Joint shoulder = body.Joints[KeyJoint1];
            Joint elbow = body.Joints[KeyJoint2];
            Joint wrist = body.Joints[KeyJoint3];

            Vector2D shoulderPos = shoulder.Position2D;
            Vector2D elbowPos = elbow.Position2D;
            Vector2D wristPos = wrist.Position2D;

            float angle = Calculations.Angle(shoulderPos, elbowPos, wristPos);

            _value = angle;
            _angleStart = shoulder.Position2D;
            _angleCenter = elbow.Position2D;
            _angleEnd = wrist.Position2D;
        }
    }
}