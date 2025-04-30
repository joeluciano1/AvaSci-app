using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class StepRightAngle : StepLeftAngle
    {
        public  StepRightAngle()
        {
            Type = MeasurementType.StepRightAngle;
            KeyJoint1 = JointType.AnkleRight;
        }

        public override void Update(Body body)
        {
            base.Update(body);
        }
    }
}