using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class AnkleLeftAbduction : Measurement
{
    public AnkleLeftAbduction()
    {
        Type = MeasurementType.AnkleLeftAbduction;

        KeyJoint1 = JointType.AnkleLeft;
        KeyJoint2 = JointType.FootLeft;
        KeyJoint3 = JointType.FootLeft;
    }

    public override void Update(Body body)
    {
        Joint ankle = body.Joints[KeyJoint1];
        Joint foot = body.Joints[KeyJoint2];

        Vector3D hip3D = ankle.Position3D;
        Vector3D knee3D = foot.Position3D;

        float angle = Calculations.Rotation(hip3D, knee3D, Plane.Sagittal);

        if (knee3D.Y < hip3D.Y)
        {
            angle = 180.0f - angle;
        }

        _value = angle;
        _angleStart = foot.Position2D;
        _angleCenter = ankle.Position2D;
        _angleEnd = new Vector2D(ankle.Position2D.X, foot.Position2D.Y);
    }
}
