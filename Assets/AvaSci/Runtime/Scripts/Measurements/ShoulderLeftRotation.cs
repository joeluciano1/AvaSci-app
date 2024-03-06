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
            Joint elbow = body.Joints[KeyJoint2];
            Joint wrist = body.Joints[KeyJoint3];

            Vector3D elbow3D = elbow.Position3D;
            Vector3D wrist3D = wrist.Position3D;

            float angle = 90.0f - Calculations.Rotation(elbow3D, wrist3D, Plane.Coronal);

            if (wrist3D.Y > elbow3D.Y) angle = -angle;

            _value = angle;
            _angleStart = elbow.Position2D;
            _angleCenter = wrist.Position2D;
            _angleEnd = new Vector2D(elbow.Position2D.X, wrist.Position2D.Y);
        }
    }
}