using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the abduction angle of the left shoulder.
    /// </summary>
    public class ShoulderLeftAbduction : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShoulderLeftAbduction"/>.
        /// </summary>
        public ShoulderLeftAbduction()
        {
            Type = MeasurementType.ShoulderLeftAbduction;
            
            KeyJoint1 = JointType.ShoulderLeft;
            KeyJoint2 = JointType.ElbowLeft;
            KeyJoint3 = JointType.HipLeft;
        }

        public override void Update(Body body)
        {
            Joint shoulder = body.Joints[KeyJoint1];
            Joint elbow = body.Joints[KeyJoint2];
            Joint hip = body.Joints[KeyJoint3];

            Vector3D shoulderPos = shoulder.Position2D;
            Vector3D elbowPos = elbow.Position2D;
            Vector3D hipPos = hip.Position2D;

            float angle = Calculations.Angle(hipPos, shoulderPos, elbowPos);

            _value = angle;
            _angleStart = elbow.Position2D;
            _angleCenter = shoulder.Position2D;
            _angleEnd = hip.Position2D;
        }
    }
}