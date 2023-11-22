using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the flexion angle of the right hip.
    /// </summary>
    public class HipRightFlexion : HipLeftFlexion
    {
        /// <summary>
        /// Creates a new instance of <see cref="HipRightFlexion"/>.
        /// </summary>
        public HipRightFlexion()
        {
            Type = MeasurementType.HipRightFlexion;
            
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