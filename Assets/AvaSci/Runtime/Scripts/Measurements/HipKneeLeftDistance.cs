using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class HipKneeLeftDistance : Measurement
{
    public HipKneeLeftDistance()
    {
        Type = MeasurementType.HipKneeLeftDistance;

        KeyJoint1 = JointType.HipLeft;
        KeyJoint2 = JointType.KneeLeft;
        KeyJoint3 = JointType.KneeLeft;
    }

    public override void Update(Body body)
    {
        Joint hipLeft = body.Joints[KeyJoint1];
        Joint kneeLeft = body.Joints[KeyJoint2];

        Vector3D hipRight3D = hipLeft.Position3D;
        Vector3D kneeRight3D = kneeLeft.Position3D;

        float difference = Math.Abs(hipRight3D.X - kneeRight3D.X);
        float miliMetersValue = difference * 1000;
        _value = miliMetersValue;
        _angleStart = hipLeft.Position2D;
        _angleCenter = kneeLeft.Position2D;
        _angleEnd = kneeLeft.Position2D;
    }
}
