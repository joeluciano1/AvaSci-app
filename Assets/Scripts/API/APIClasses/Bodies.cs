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

public class VideoSaveBody
{
    public string FileName { get; set; }
    public string FileData { get; set; }
}

public class ReportRecordBody
{
    public string UserName { get; set; }
    public string VideoURL { get; set; }
    public string? ReportURL { get; set; }
}

public class SubscriptionBody
{
    public string UserEmail { get; set; }
    public bool IsMonthlySub { get; set; }
    public string Receipt { get; set; }
}

public class ReportDeleteBody
{
    public string UserId { get; set; }
    public string VideoURL { get; set; }
}

public class DeleteAccountBody
{
    public string UserID { get; set; }
}

public class GetReportsBody
{
    public string UserID { get; set; }
}