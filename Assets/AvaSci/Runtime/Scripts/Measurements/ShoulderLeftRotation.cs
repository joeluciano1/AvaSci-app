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
            base.Update(body);
            if (ReferenceManager.instance.ignoreLowConfidenceJoints &&
                ((body.Joints[KeyJoint1] != null && body.Joints[KeyJoint1].Confidence < 0.3f) ||
                 (body.Joints[KeyJoint2] != null && body.Joints[KeyJoint2].Confidence < 0.3f) ||
                 (body.Joints[KeyJoint3] != null && body.Joints[KeyJoint3].Confidence < 0.3f)))
            {
                return; // Prevent further execution
            }
            Joint shoulder = body.Joints[KeyJoint1];
            Joint elbow = body.Joints[KeyJoint2];

            Vector3D shoulder3D = shoulder.Position3D;
            Vector3D elbow3D = elbow.Position3D;

            float angle = Calculations.Rotation(shoulder3D, elbow3D, Plane.Transverse);

            if (elbow3D.Y > shoulder3D.Y) angle = -angle;

            _value = angle;
            _angleStart = shoulder.Position2D;
            _angleCenter = elbow.Position2D;
            _angleEnd = new Vector2D(shoulder.Position2D.X, elbow.Position2D.Y);
        }
    }
}