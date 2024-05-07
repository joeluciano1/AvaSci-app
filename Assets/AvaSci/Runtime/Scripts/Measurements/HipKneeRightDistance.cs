

using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class HipKneeRightDistance : HipKneeLeftDistance
{
    public HipKneeRightDistance()
    {
        Type = MeasurementType.HipKneeRightDistance;

        KeyJoint1 = JointType.HipRight;
        KeyJoint2 = JointType.KneeRight;
        KeyJoint3 = JointType.KneeRight;
    }

    public override void Update(Body body)
    {
        base.Update(body);
    }
}
