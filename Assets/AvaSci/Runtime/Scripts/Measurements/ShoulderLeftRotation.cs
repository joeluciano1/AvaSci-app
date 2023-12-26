using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the rotation angle of the left shoulder.
    /// </summary>
    public class ShoulderLeftRotation : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShoulderLeftRotation"/>.
        /// </summary>
        public ShoulderLeftRotation()
        {
            Type = MeasurementType.ShoulderLeftRotation;
            
            KeyJoint1 = JointType.ShoulderLeft;
            KeyJoint2 = JointType.ElbowLeft;
            KeyJoint3 = JointType.WristLeft;
        }

        public override void Update(Body body)
        {
            Joint shoulder = body.Joints[KeyJoint1];
            Joint elbow = body.Joints[KeyJoint2];
            Joint wrist = body.Joints[KeyJoint3];

            Vector3D elbowPos = elbow.Position3D;
            Vector3D wristPos = wrist.Position3D;

            float angle = 90.0f - Calculations.Rotation(elbowPos, wristPos, Plane.Coronal);

            if (wristPos.Y > elbowPos.Y) angle = -angle;

            _value = angle;
            _angleStart = shoulder.Position2D;
            _angleCenter = elbow.Position2D;
            _angleEnd = wrist.Position2D;
        }
    }
}