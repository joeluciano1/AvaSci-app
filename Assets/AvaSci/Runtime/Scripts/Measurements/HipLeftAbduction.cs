using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the abduction angle of the left hip.
    /// </summary>
    public class HipLeftAbduction : Measurement
    {
        /// <summary>
        /// Creates a new instance of <see cref="HipLeftAbduction"/>.
        /// </summary>
        public HipLeftAbduction()
        {
            Type = MeasurementType.HipLeftAbduction;
            
            KeyJoint1 = JointType.HipLeft;
            KeyJoint2 = JointType.KneeLeft;
            KeyJoint3 = JointType.AnkleLeft;
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
            Joint hip = body.Joints[KeyJoint1];
            Joint knee = body.Joints[KeyJoint2];

            Vector3D hip3D = hip.Position3D;
            Vector3D knee3D = knee.Position3D;

            float angle = Calculations.Rotation(hip3D, knee3D, Plane.Sagittal);

            if (knee3D.Y < hip3D.Y)
            {
                angle = 180.0f - angle;
            }

            _value = angle;
            _angleStart = knee.Position2D;
            _angleCenter = hip.Position2D;
            _angleEnd = new Vector2D(hip.Position2D.X, knee.Position2D.Y);
            
            if(ResearchMeasurementManager.instance.leftLeg)
                ResearchMeasurementManager.instance.leftKneeAbdValue = _value;
            else
                ResearchMeasurementManager.instance.rightKneeAbdValue = _value;
        }
    }
}