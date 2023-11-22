using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the lateral flexion angle of the neck.
    /// </summary>
    public class NeckLateralFlexion : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="NeckLateralFlexion"/>.
        /// </summary>
        public NeckLateralFlexion()
        {
            Type = MeasurementType.NeckLateralFlexion;
            
            KeyJoint1 = JointType.EyeLeft;
            KeyJoint2 = JointType.Neck;
            KeyJoint3 = JointType.EyeRight;
        }

        public override void Update(Body body)
        {
            Joint neck = body.Joints[KeyJoint2];
            Joint eyeLeft = body.Joints[KeyJoint1];
            Joint eyeRight = body.Joints[KeyJoint3];

            Vector3D neck3D = neck.Position3D;
            Vector3D eyeLeft3D = eyeLeft.Position3D;
            Vector3D eyeRight3D = eyeRight.Position3D;

            Vector2D eyeCenter2D = (eyeLeft.Position2D + eyeRight.Position2D) / 2.0f;
            Vector3D eyeCenter3D = (eyeLeft3D + eyeRight3D) / 2.0f;

            float angle = Calculations.Rotation(neck3D, eyeCenter3D, Plane.Sagittal);

            if (eyeCenter3D.X > neck3D.X) angle = -angle;

            _value = angle;
            _angleStart = eyeCenter2D;
            _angleCenter = neck.Position2D;
            _angleEnd = new Vector2D(neck.Position2D.X, eyeCenter2D.Y);
        }
    }
}