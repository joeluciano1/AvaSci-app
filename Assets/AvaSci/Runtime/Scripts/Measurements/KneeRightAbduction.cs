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

            KeyJoint1 = JointType.KneeRight;
            KeyJoint2 = JointType.AnkleRight;
            KeyJoint3 = JointType.FootRight;
        }

        public override void Update(Body body)
        {
            base.Update(body);
        }
    }
}