using System;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class AnkleHipLeftDifference : Measurement
{
    public AnkleHipLeftDifference()
    {
        Type = MeasurementType.HipAnkleHipKneeLeftAbductionDifference;

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

        if (knee3D.Y < hip3D.Y)
        {
            angleHipABD = 180.0f - angleHipABD;
        }

        Joint ankle = body.Joints[JointType.AnkleLeft];
        Joint foot = body.Joints[JointType.FootLeft];

        Vector3D ankle3D = ankle.Position3D;
        Vector3D foot3D = foot.Position3D;

        float angleAnkleABD = Calculations.Rotation(hip3D, ankle3D, Plane.Sagittal);
        // UnityEngine.Debug.Log("AngleValue "+angleAnkleABD);
        if (ankle3D.Y < hip3D.Y)
        {
            angleAnkleABD = 180.0f - angleAnkleABD;
        }

        Joint hipLeft = body.Joints[KeyJoint1];
        Joint ankleLeft = body.Joints[KeyJoint2];
        Joint AnkleLeft = body.Joints[KeyJoint3];

        Vector3D hipLeft3D = hipLeft.Position3D;
        Vector3D ankleLeft3D = ankleLeft.Position3D;
        // Vector3D hip3D = hip.Position2D;

        float difference = angleHipABD - angleAnkleABD;

        _value = difference;
        _angleStart = hipLeft.Position2D;
        _angleCenter = (knee.Position2D + hipLeft.Position2D) / 2;
        _angleEnd = ankleLeft.Position2D;
    }
}
