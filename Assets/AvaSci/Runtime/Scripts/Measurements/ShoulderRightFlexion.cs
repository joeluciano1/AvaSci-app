using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the flexion angle of the left shoulder.
    /// </summary>
    public class ShoulderRightFlexion : ShoulderLeftFlexion
    {
        /// <summary>
        /// Creates a new instance of <see cref="ElbowRightFlexion"/>.
        /// </summary>
        public ShoulderRightFlexion()
        {
            Type = MeasurementType.ShoulderRightFlexion;

            KeyJoint1 = JointType.Neck;
            KeyJoint2 = JointType.ShoulderRight;
            KeyJoint3 = JointType.ElbowRight;
        }

        public override void Update(Body body)
        {
            base.Update(body);
        }
    }
}