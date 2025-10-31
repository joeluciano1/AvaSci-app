using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class XiphoidAngle : Measurement
    {
        public XiphoidAngle()
        {
            Type = MeasurementType.XiphoidAngle;
            
            KeyJoint1 = JointType.Xiphoid;
            KeyJoint2 = JointType.Pelvis;
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
            Joint pelvis = body.Joints[KeyJoint2];

            Vector3D Xiphoid3D = Xiphoid.Position3D;
            Vector3D pelvisPosition3D = pelvis.Position3D;

            float angle = Calculations.Rotation(Xiphoid3D, pelvisPosition3D, ReferenceManager.instance.XiphoidRotationPlane);

            if (pelvisPosition3D.X > Xiphoid3D.X) angle = -angle;
            _value = angle;
            
            _angleStart = Xiphoid.Position2D;
            _angleCenter = Xiphoid.Position2D;
            _angleEnd = new Vector2D(Xiphoid.Position2D.X, pelvis.Position2D.Y);
        }
    }
}