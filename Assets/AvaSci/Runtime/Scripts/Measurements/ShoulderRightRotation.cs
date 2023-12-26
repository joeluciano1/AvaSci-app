using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the rotation angle of the right shoulder.
    /// </summary>
    public class ShoulderRightRotation : ShoulderLeftRotation
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShoulderRightRotation"/>.
        /// </summary>
        public ShoulderRightRotation()
        {
            Type = MeasurementType.ShoulderRightRotation;
            
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