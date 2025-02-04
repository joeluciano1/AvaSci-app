using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class AnkleLeftAbduction : Measurement
{
    public AnkleLeftAbduction()
    {
        Type = MeasurementType.AnkleLeftAbduction;

        KeyJoint1 = JointType.HipLeft;
        KeyJoint2 = JointType.AnkleLeft;
        KeyJoint3 = JointType.FootLeft;
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
        Joint hip = body.Joints[KeyJoint1];
        Joint ankle = body.Joints[KeyJoint2];

        Vector3D hip3D = hip.Position3D;
        Vector3D ankle3D = ankle.Position3D;

        float angle = Calculations.Rotation(hip3D, ankle3D, Plane.Coronal);

        if (ankle3D.Y < hip3D.Y)
        {
            angle = 180.0f - angle;
        }

        _value = angle;
        _angleStart = hip.Position2D;
        _angleCenter = ankle.Position2D;
        _angleEnd = new Vector2D(hip.Position2D.X, ankle.Position2D.Y);
        if(ResearchMeasurementManager.instance.leftLeg)
            ResearchMeasurementManager.instance.leftAnkleAbdValue = _value;
        else
            ResearchMeasurementManager.instance.rightAnkleAbdValue = _value;
    }
}
