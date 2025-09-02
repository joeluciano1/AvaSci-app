using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class XiphoidAngle : Measurement
    {
        public XiphoidAngle()
        {
            Type = MeasurementType.XiphoidAngle;
            
            KeyJoint1 = JointType.Xiphoid;
            KeyJoint2 = JointType.Waist;
            KeyJoint3 = JointType.Pelvis;
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
            Joint Xiphoid = body.Joints[KeyJoint1];
            Joint Waist = body.Joints[KeyJoint2];

            Vector3D Xiphoid3D = Xiphoid.Position3D;
            Vector3D waist3D = Waist.Position3D;

            float angle = Calculations.Rotation(Xiphoid3D, waist3D, ReferenceManager.instance.XiphoidRotationPlane);

            if (waist3D.Y > Xiphoid3D.Y) angle = -angle;

            _value = angle;
            _angleStart = Xiphoid.Position2D;
            _angleCenter = Xiphoid.Position2D;
            _angleEnd = new Vector2D(Xiphoid.Position2D.X, Waist.Position2D.Y);
        }
    }
}