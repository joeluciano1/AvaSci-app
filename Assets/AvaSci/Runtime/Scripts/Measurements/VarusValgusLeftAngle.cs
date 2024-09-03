using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class VarusValgusLeftAngle : Measurement
{
  public VarusValgusLeftAngle()
  {
    Type = MeasurementType.VarusValgusLeftAngleDistance;
     KeyJoint1 = JointType.HipLeft;
        KeyJoint2 = JointType.KneeLeft;
        KeyJoint3 = JointType.KneeLeft;
  }
    public override void Update(Body body)
    {
        base.Update(body);
        float myval = 0;
        if (ReferenceManager.instance.LeftAngleDifference == null || ReferenceManager.instance.LeftDistance == null)
        {
            UnityEngine.Debug.Log("Left is null");
            myval = 0;
        }
        else
        {
            myval = (float)(ReferenceManager.instance.LeftDistance.Angle * Math.Sin((double)ReferenceManager.instance.LeftAngleDifference.Angle*3.14159/180));
            
        }
        _value = myval;
        Joint hipLeft = body.Joints[KeyJoint1];
        Joint kneeLeft = body.Joints[KeyJoint2];
        _angleStart = hipLeft.Position2D;
        _angleCenter = kneeLeft.Position2D;
        _angleEnd = kneeLeft.Position2D;
        
        ResearchMeasurementManager.instance.leftDisValue = myval;
         
    }
}
