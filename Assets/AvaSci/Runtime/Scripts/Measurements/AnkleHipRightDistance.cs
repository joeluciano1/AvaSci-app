using System.Collections;
using System.Collections.Generic;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

public class AnkleHipRightDistance : AnkleHipLeftDistance
{
    public AnkleHipRightDistance()
    {
        Type = MeasurementType.AnkleHipLeftDistance;

        KeyJoint1 = JointType.HipRight;
        KeyJoint2 = JointType.AnkleRight;
        KeyJoint3 = JointType.AnkleRight;
    }

    public override void Update(Body body)
    {
        base.Update(body);
    }
}
