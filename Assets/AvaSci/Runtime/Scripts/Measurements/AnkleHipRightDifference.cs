using System;
using System.Collections;
using System.Collections.Generic;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class AnkleHipRightDifference : AnkleHipLeftDifference
{
    public AnkleHipRightDifference()
    {
        Type = MeasurementType.HipAnkleHipKneeRightAbductionDifference;

        KeyJoint1 = JointType.HipRight;
        KeyJoint2 = JointType.AnkleRight;
        KeyJoint3 = JointType.AnkleRight;
    }

    public override void Update(Body body)
    {
        Joint hip = body.Joints[JointType.HipRight];
        Joint knee = body.Joints[JointType.KneeRight];

        Vector3D hip3D = hip.Position3D;
        Vector3D knee3D = knee.Position3D;

        float angleHipABD = Calculations.Rotation(hip3D, knee3D, Plane.Coronal);

        if (knee3D.Y < hip3D.Y)
        {
            angleHipABD = 180.0f - angleHipABD;
        }

        Joint ankle = body.Joints[JointType.AnkleRight];
        Joint foot = body.Joints[JointType.FootRight];

        Vector3D ankle3D = ankle.Position3D;
        Vector3D foot3D = foot.Position3D;

        float angleAnkleABD = Calculations.Rotation(hip3D, ankle3D, Plane.Coronal);

        if (ankle3D.Y < hip3D.Y)
        {
            angleAnkleABD = 180.0f - angleAnkleABD;
        }

        Joint hipRight = body.Joints[KeyJoint1];
        Joint ankleRight = body.Joints[KeyJoint2];
        Joint AnkleRight = body.Joints[KeyJoint3];

        Vector3D hipRight3D = hipRight.Position3D;
        Vector3D ankleRight3D = ankleRight.Position3D;
        // Vector3D hip3D = hip.Position2D;

        float difference = angleHipABD - angleAnkleABD;

        _value = difference;
        _angleStart = hipRight.Position2D;
        _angleCenter = (knee.Position2D + hipRight.Position2D) / 2;
        _angleEnd = ankleRight.Position2D;
    }
}
