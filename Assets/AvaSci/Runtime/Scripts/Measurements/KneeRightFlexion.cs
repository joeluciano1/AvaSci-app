using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the flexion angle of the right knee.
    /// </summary>
    public class KneeRightFlexion : KneeLeftFlexion
    {
        /// <summary>
        /// Creates a new instance of <see cref="KneeRightFlexion"/>.
        /// </summary>
        public KneeRightFlexion()
        {
            Type = MeasurementType.KneeRightFlexion;
            
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