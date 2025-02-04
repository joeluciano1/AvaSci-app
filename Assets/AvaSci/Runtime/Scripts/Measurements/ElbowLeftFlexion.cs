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
            Joint wrist = body.Joints[KeyJoint3];

            Vector3D shoulder3D = shoulder.Position3D;
            Vector3D elbow3D = elbow.Position3D;
            Vector3D wrist3D = wrist.Position3D;

            float angle = Calculations.Angle(shoulder3D, elbow3D, wrist3D);

            _value = angle;
            _angleStart = shoulder.Position2D;
            _angleCenter = elbow.Position2D;
            _angleEnd = wrist.Position2D;
        }
    }
}