using LightBuzz.BodyTracking;
namespace LightBuzz.AvaSci.Measurements
{
    public class KneeRightAbduction : KneeLeftAbduction
    {
        /// <summary>
        /// Creates a new instance of <see cref="KneeRightAbduction"/>.
        /// </summary>
        public KneeRightAbduction()
        {
            Type = MeasurementType.KneeRightAbduction;

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