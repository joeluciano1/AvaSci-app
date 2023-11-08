using System;
using System.Collections.Generic;

public class Bodies
{
    
}
public class SignupBody
{
    public string email { get; set; }
    public string password { get; set; }
    public int countryId { get; set; }
    public int genderId { get; set; }
    public DateTime birthDate { get; set; }
    public string reasonForDownload { get; set; }
    public string interests { get; set; }
    public string AdvancedSurvey { get; set; }
}

public class LoginBody
{
    public string email { get; set; }
    public string password { get; set; }
}

public class Joint
{
    public JointType JointType { get; set; }
    public bool Healthy { get; set; }
    public bool ExperiencePain { get; set; }
    public bool ResultOfInjury { get; set; }
    public bool PreviousSurgery { get; set; }
}

public class JointBody
{
    public List<Joint> Joints = new List<Joint>();
    public string Other { get; set; }
    public string LevelOfFitness { get; set; }
}

public enum JointType
{
    Cervical,
    ShoulderRight,
    ShoulderLeft,
    Spine,
    HipRight,
    HipLeft,
    KneeRight,
    KneeLeft,
    AnkleRight,
    AnkleLeft,
}

