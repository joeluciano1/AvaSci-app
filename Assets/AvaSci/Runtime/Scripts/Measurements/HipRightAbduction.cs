using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the abduction angle of the right hip.
    /// </summary>
    public class HipRightAbduction : HipLeftAbduction
    {
        /// <summary>
        /// Creates a new instance of <see cref="HipRightAbduction"/>.
        /// </summary>
        public HipRightAbduction()
        {
            Type = MeasurementType.HipRightAbduction;
            
            KeyJoint1 = JointType.HipRight;
            KeyJoint2 = JointType.KneeRight;
            KeyJoint3 = JointType.AnkleRight;
        }

        public override void Update(Body body)
        {
            base.Update(body);
        }
    }
}