using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the abduction angle of the right shoulder.
    /// </summary>
    public class ShoulderRightAbduction : ShoulderLeftAbduction
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShoulderRightAbduction"/>.
        /// </summary>
        public ShoulderRightAbduction()
        {
            Type = MeasurementType.ShoulderRightAbduction;
            
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