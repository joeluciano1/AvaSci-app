using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class PelvisAngle : Measurement
    {
        public PelvisAngle()
        {
            Type = MeasurementType.PelvisAngle;

            KeyJoint1 = JointType.Waist;
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
            Joint nose = body.Joints[KeyJoint1];
            Joint head = body.Joints[KeyJoint3];

            Vector3D nose3D = nose.Position3D;
            Vector3D head3D = head.Position3D;

            float angle = Calculations.Rotation(nose3D, head3D, Plane.Sagittal);

            if (nose3D.X > head3D.X) angle = -angle;

            _value = angle * 1.52f;
            _angleStart = nose.Position2D;
            _angleCenter = head.Position2D;
            _angleEnd = new Vector2D(head.Position2D.X, nose.Position2D.Y);

            
            ResearchMeasurementManager.instance.pelvisAngleValue = _value;
        
        
        }
    }
}
