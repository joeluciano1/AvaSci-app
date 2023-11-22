using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the flexion angle of the right elbow.
    /// </summary>
    public class ElbowRightFlexion : ElbowLeftFlexion
    {
        /// <summary>
        /// Creates a new instance of <see cref="ElbowRightFlexion"/>.
        /// </summary>
        public ElbowRightFlexion()
        {
            Type = MeasurementType.ElbowRightFlexion;
            
            KeyJoint1 = JointType.ShoulderRight;
            KeyJoint2 = JointType.ElbowRight;
            KeyJoint3 = JointType.WristRight;
        }

        public override void Update(Body body)
        {
            base.Update(body);
        }
    }
}