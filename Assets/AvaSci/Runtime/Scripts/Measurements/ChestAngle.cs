using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class ChestAngle : Measurement
    {
        public ChestAngle()
        {
            Type = MeasurementType.ChestAngle;
            
            KeyJoint1 = JointType.Chest;
            KeyJoint2 = JointType.Pelvis;
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
            Joint pelvis = body.Joints[KeyJoint2];

            Vector3D chest3D = chest.Position3D;
            Vector3D pelvisPosition3D = pelvis.Position3D;

            float angle = Calculations.Rotation(chest3D, pelvisPosition3D, ReferenceManager.instance.ChestRotationPlane);

            if (pelvisPosition3D.X > chest3D.X) angle = -angle;
            _value = angle;
            
            _angleStart = chest.Position2D;
            _angleCenter = chest.Position2D;
            _angleEnd = new Vector2D(chest.Position2D.X, pelvis.Position2D.Y);
        }
    }
}