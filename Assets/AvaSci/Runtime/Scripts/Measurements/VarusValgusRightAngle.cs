using System;
using System.Collections;
using System.Collections.Generic;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;


public class VarusValgusRightAngle : Measurement
{
   
  public VarusValgusRightAngle()
  {
    Type = MeasurementType.VarusValgusRightAngleDistance;

     KeyJoint1 = JointType.HipRight;
        KeyJoint2 = JointType.KneeRight;
        KeyJoint3 = JointType.KneeRight;
  }
    public override void Update(Body body)
    {
        base.Update(body);
        float myval = 0;
        if (ReferenceManager.instance.RightAngleDifference == null || ReferenceManager.instance.RightDistance == null)
        {
            myval = 0;
            UnityEngine.Debug.Log("Right is null");
        }
        else
        {
            myval = (float)(ReferenceManager.instance.RightDistance?.Angle * Math.Sin((double)ReferenceManager.instance.RightAngleDifference?.Angle*3.14159/180));

        }
        _value = myval;
    Joint hipLeft = body.Joints[KeyJoint1];
    Joint kneeLeft = body.Joints[KeyJoint2];
    _angleStart = hipLeft.Position2D;
    _angleCenter = kneeLeft.Position2D;
    _angleEnd = kneeLeft.Position2D;
    }
}
