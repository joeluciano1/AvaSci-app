using System.Collections;
using System.Collections.Generic;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

public class HipKneeRightDistance : HipKneeLeftDistance
{
    public HipKneeRightDistance()
    {
        Type = MeasurementType.HipKneeRightDistance;

        KeyJoint1 = JointType.HipRight;
        KeyJoint2 = JointType.KneeRight;
        KeyJoint3 = JointType.KneeRight;
    }

    // Update is called once per frame
    public override void Update(Body body)
    {
        base.Update(body);
    }
}
