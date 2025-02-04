using System;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class HipKneeLeftDifference : Measurement
{
    public HipKneeLeftDifference()
    {
        Type = MeasurementType.HipKneeLeftDifference;

        KeyJoint1 = JointType.HipLeft;
        KeyJoint2 = JointType.KneeLeft;
        KeyJoint3 = JointType.KneeLeft;
    }

    public override void Update(Body body)
    {
        base.Update(body);
        if (ReferenceManager.instance.ignoreLowConfidenceJoints &&
            ((body.Joints[KeyJoint1] != null && body.Joints[KeyJoint1].Confidence < 0.3f) ||
             (body.Joints[KeyJoint2] != null && body.Joints[KeyJoint2].Confidence < 0.3f) ||
             (body.Joints[KeyJoint3] != null && body.Joints[KeyJoint3].Confidence < 0.3f)))
        {
            return; // Prevent further execution
        }
        Joint hipLeft = body.Joints[KeyJoint1];
        Joint kneeLeft = body.Joints[KeyJoint2];

        Vector3D hipLeft3D = hipLeft.Position3D;
        Vector3D kneeLeft3D = kneeLeft.Position3D;

        float angleHipABD = Calculations.Rotation(hipLeft3D, kneeLeft3D, Plane.Coronal);

        // if (kneeLeft3D.Y < hipLeft3D.Y)
        // {
        //     angleHipABD = 180.0f - angleHipABD;
        // }

        Joint shoulder = body.Joints[JointType.KneeLeft];
        Joint elbow = body.Joints[JointType.AnkleLeft];
        Joint hip = body.Joints[JointType.FootLeft];

        Vector3D shoulder3D = shoulder.Position2D;
        Vector3D elbow3D = elbow.Position2D;
        Vector3D hip3D = hip.Position2D;

        float angle = Calculations.Angle(hip3D, shoulder3D, elbow3D);

        // if (shoulder3D.Y < hip3D.Y)
        // {
        //     angle = 180.0f - angle;
        // }

        float difference = Math.Abs(angleHipABD - angle);

        _value = difference;
        _angleStart = hipLeft.Position2D;
        _angleCenter = (kneeLeft.Position2D + hipLeft.Position2D) / 2;
        _angleEnd = kneeLeft.Position2D;
    }
}
