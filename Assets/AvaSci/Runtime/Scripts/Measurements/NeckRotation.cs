using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the rotation angle of the neck.
    /// </summary>
    public class NeckRotation : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="NeckRotation"/>.
        /// </summary>
        public NeckRotation()
        {
            Type = MeasurementType.NeckRotation;
            
            KeyJoint1 = JointType.Nose;
            KeyJoint2 = JointType.Neck;
            KeyJoint3 = JointType.Neck;
        }

        public override void Update(Body body)
        {
            Joint nose = body.Joints[KeyJoint1];
            Joint head = body.Joints[KeyJoint3];

            Vector3D nosePos = nose.Position3D;
            Vector3D headPos = head.Position3D;

            float angle = Calculations.Rotation(nosePos, headPos, Plane.Sagittal);

            if (nosePos.X > headPos.X) angle = -angle;

            _value = angle * 1.52f;
            _angleStart = nose.Position2D;
            _angleCenter = head.Position2D;
            _angleEnd = new Vector2D(head.Position2D.X, nose.Position2D.Y);
        }
    }
}