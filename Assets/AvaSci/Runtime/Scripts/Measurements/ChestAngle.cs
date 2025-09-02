using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class ChestAngle : Measurement
    {
        public ChestAngle()
        {
            Type = MeasurementType.ChestAngle;
            
            KeyJoint1 = JointType.Chest;
            KeyJoint2 = JointType.Xiphoid;
            KeyJoint3 = JointType.Waist;
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
            Joint chest = body.Joints[KeyJoint1];
            Joint xiphoid = body.Joints[KeyJoint2];

            Vector3D chest3D = chest.Position3D;
            Vector3D xiphoid3D = xiphoid.Position3D;

            float angle = Calculations.Rotation(chest3D, xiphoid3D, ReferenceManager.instance.ChestRotationPlane);

            if (xiphoid3D.Y > chest3D.Y) angle = -angle;

            _value = angle;
            _angleStart = chest.Position2D;
            _angleCenter = chest.Position2D;
            _angleEnd = new Vector2D(chest.Position2D.X, xiphoid.Position2D.Y);
        }
    }
}