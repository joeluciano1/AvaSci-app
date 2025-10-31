using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class WaistAngle : Measurement
    {
        public WaistAngle()
        {
            Type = MeasurementType.WaistAngle;
            
            KeyJoint1 = JointType.Waist;
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
            Joint waist = body.Joints[KeyJoint1];
            Joint pelvis = body.Joints[KeyJoint2];

            Vector3D waist3D = waist.Position3D;
            Vector3D pelvis3D = pelvis.Position3D;

            float angle = Calculations.Rotation(waist3D, pelvis3D, ReferenceManager.instance.WaistRotationPlane);

            if (pelvis3D.X > waist3D.X) angle = -angle;

            _value = angle;
            _angleStart = waist.Position2D;
            _angleCenter = waist.Position2D;
            _angleEnd = new Vector2D(waist.Position2D.X, pelvis.Position2D.Y);
        }
    }
}