using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class AnkleRightAbduction : AnkleLeftAbduction
{
    public AnkleRightAbduction()
    {
        Type = MeasurementType.AnkleRightAbduction;

        KeyJoint1 = JointType.HipRight;
        KeyJoint2 = JointType.AnkleRight;
        KeyJoint3 = JointType.FootRight;
    }
    public override void Update(Body body)
    {
        base.Update(body);
    }
}
