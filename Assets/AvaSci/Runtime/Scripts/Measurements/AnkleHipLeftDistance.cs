using System;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class AnkleHipLeftDistance : Measurement
{
    public AnkleHipLeftDistance()
    {
        Type = MeasurementType.AnkleHipLeftDistance;

        KeyJoint1 = JointType.HipLeft;
        KeyJoint2 = JointType.AnkleLeft;
        KeyJoint3 = JointType.AnkleLeft;
    }

    public override void Update(Body body)
    {
        Joint hipLeft = body.Joints[KeyJoint1];
        Joint ankleLeft = body.Joints[KeyJoint2];
        Joint AnkleLeft = body.Joints[KeyJoint3];

        Vector3D hipLeft3D = hipLeft.Position3D;
        Vector3D ankleLeft3D = ankleLeft.Position3D;
        // Vector3D hip3D = hip.Position2D;

        float difference = Math.Abs(ankleLeft3D.X) - Math.Abs(hipLeft3D.X);

        _value = difference;
        _angleStart = hipLeft.Position2D;
        _angleCenter = (ankleLeft.Position2D + hipLeft.Position2D) / 2;
        _angleEnd = ankleLeft.Position2D;
    }
}
