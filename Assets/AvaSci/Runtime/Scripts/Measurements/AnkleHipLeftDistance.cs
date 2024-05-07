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

        Joint hip = body.Joints[JointType.HipLeft];
        Joint knee = body.Joints[JointType.KneeLeft];

        Vector3D hip3D = hip.Position3D;
        Vector3D knee3D = knee.Position3D;

        float angleHipABD = Calculations.Rotation(hip3D, knee3D, Plane.Sagittal);

        Joint shoulder = body.Joints[JointType.KneeLeft];
        Joint elbow = body.Joints[JointType.AnkleLeft];
        Joint hipp = body.Joints[JointType.FootLeft];

        Vector3D shoulder3D = shoulder.Position2D;
        Vector3D elbow3D = elbow.Position2D;
        Vector3D hipp3D = hip.Position2D;

        float angleKneeABD = Calculations.Angle(hip3D, shoulder3D, elbow3D);

        if (knee3D.Y < hip3D.Y)
        {
            angleHipABD = 180.0f - angleHipABD;
        }

        Joint hipLeft = body.Joints[KeyJoint1];
        Joint ankleLeft = body.Joints[KeyJoint2];
        Joint AnkleLeft = body.Joints[KeyJoint3];

        Vector3D hipLeft3D = hipLeft.Position3D;
        Vector3D ankleLeft3D = ankleLeft.Position3D;
        // Vector3D hip3D = hip.Position2D;

        float difference = Math.Abs(angleHipABD - angleKneeABD);

        _value = difference;
        _angleStart = hipLeft.Position2D;
        _angleCenter = (ankleLeft.Position2D + hipLeft.Position2D) / 2;
        _angleEnd = ankleLeft.Position2D;
    }
}
