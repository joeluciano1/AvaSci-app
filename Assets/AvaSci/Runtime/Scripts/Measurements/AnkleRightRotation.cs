using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class AnkleRightRotation : AnkleLeftRotation
    {
        public AnkleRightRotation()
        {
            Type = MeasurementType.AnkleRightRotation;

            KeyJoint1 = JointType.KneeRight;
            KeyJoint2 = JointType.AnkleRight;
            KeyJoint3 = JointType.AnkleRight;
        }

        public override void Update(Body body)
        {
            base.Update(body);
        }
    }
}