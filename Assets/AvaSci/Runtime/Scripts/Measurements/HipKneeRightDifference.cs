using System;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class HipKneeRightDifference : Measurement
{
    public HipKneeRightDifference()
    {
        Type = MeasurementType.HipKneeRightDifference;

        KeyJoint1 = JointType.HipRight;
        KeyJoint2 = JointType.KneeRight;
        KeyJoint3 = JointType.KneeRight;
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
        Joint hipRight = body.Joints[KeyJoint1];
        Joint kneeRight = body.Joints[KeyJoint2];

        Vector3D hipRight3D = hipRight.Position3D;
        Vector3D kneeRight3D = kneeRight.Position3D;

        float angleHipABD = Calculations.Rotation(hipRight3D, kneeRight3D, Plane.Coronal);

        // if (kneeRight3D.Y < hipRight3D.Y)
        // {
        //     angleHipABD = 180.0f - angleHipABD;
        // }

        Joint shoulder = body.Joints[JointType.KneeRight];
        Joint elbow = body.Joints[JointType.AnkleRight];
        Joint hip = body.Joints[JointType.FootRight];

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
        _angleStart = hipRight.Position2D;
        _angleCenter = (kneeRight.Position2D + hipRight.Position2D) / 2;
        _angleEnd = kneeRight.Position2D;
    }
}
