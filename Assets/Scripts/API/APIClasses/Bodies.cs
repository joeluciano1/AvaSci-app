using System;
using System.Collections.Generic;
using LightBuzz.BodyTracking;

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

public class JointData
{
    public JointType JointType { get; set; }
    public bool Healthy { get; set; }
    public bool ExperiencePain { get; set; }
    public bool ResultOfInjury { get; set; }
    public bool PreviousSurgery { get; set; }
}

public class JointBody
{
    public List<JointData> Joints = new List<JointData>();
    public string Other { get; set; }
    public string LevelOfFitness { get; set; }
}

public class SendEmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}

public class CheckCodeBody
{
    public string Email { get; set; }
    public string Code { get; set; }
}

public class ResetPasswordBody
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
}