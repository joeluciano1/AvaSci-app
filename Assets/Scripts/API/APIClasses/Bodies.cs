using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LightBuzz.BodyTracking;

public class Bodies { }

public class SignupBody
{
    public string email { get; set; }
    public string userName { get; set; }
    public string password { get; set; }
    public int countryId { get; set; }
    public int genderId { get; set; }
    public DateTime birthDate { get; set; }
    public string reasonForDownload { get; set; }
    public string interests { get; set; }
    public string AdvancedSurvey { get; set; }
}
public class CreateHtmlReportBody
{
    public long? ReportsRecordId { get; set; }
    public string HtmlReport { get; set; }
    public string CreatedBy;
}
public class GetHtmlReportRequest
{
    public long ReportRecordId { get; set; }
}
public class GetSharedRecordingRequest
{
    public string UserEmail { get; set; }

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
    public string CreatedBy { get; set; }
    public string UserName { get; set; }
    public string VideoURL { get; set; }
    public string? ReportURL { get; set; }
    public string ReportDescription { get; set; }
    public string SubjectId{ get; set; }
    public string GroupName { get; set; }
    public List<JointReading> jointReadings { get; set; }=new List<JointReading>();
    public List<TimeBasedReadingRequest> TimeBasedReadings { get; set; } = new List<TimeBasedReadingRequest>();
}
public class JointReading
	{
        public long? ReportId { get; set; }
		public string NameOfReading { get; set; }
		public float MinimumValue { get; set; }
		public float MaximumValue { get; set; }
		public float RangeValue { get; set; }
        public DateTime CreatedOn{ get; set; }
        public string VideoNameLink { get; set; }
	}
    [System.Serializable]
    public class TimeBasedReadingRequest
	{
        public long? ReportsRecordId { get; set; }
        public string UserName { get; set; }
        public string TimeOfReading { get; set; }
        public float? KneeLeftAbduction { get; set; }
        public float? KneeRightAbduction { get; set; }
        public float? PelvisAngle { get; set; }
        public float? AnkleHipLeftAbductionDifference { get; set; }
        public float? AnkleHipRightAbductionDifference { get; set; }
        public float? HipKneeRightDistance { get; set; }
        public float? HipKneeLeftDistance { get; set; }
        public float? NeckLateralFlexion { get; set; }
        public float? NeckRotation { get; set; }
        public float? ElbowLeftFlexion { get; set; }
        public float? ElbowRightFlexion { get; set; }
        public float? ShoulderLeftAbduction { get; set; }
        public float? ShoulderLeftRotation { get; set; }
        public float? ShoulderRightAbduction { get; set; }
        public float? ShoulderRightRotation { get; set; }
        public float? ShoulderLeftFlexion { get; set; }
        public float? ShoulderRightFlexion { get; set; }
        public float? HipLeftAbduction { get; set; }
        public float? HipLeftFlexion { get; set; }
        public float? HipRightAbduction { get; set; }
        public float? HipRightFlexion { get; set; }
        public float? KneeLeftFlexion { get; set; }
        public float? KneeRightFlexion { get; set; }
        public float? AnkleLeftAbduction { get; set; }
        public float? AnkleRightAbduction { get; set; }
        public float? VarusValgusRight { get; set; }
        public float? VarusValgusLeft { get; set; }
        [CanBeNull] public string VideoName { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class RReportGenerateRequest
    {
        public string Rmd { get; set; }
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

public class CreateGaitReportBody
{
    public long? ReportsRecordId { get; set; }
    public string CreatedBy { get; set; }
    public string Subject { get; set; }
    public float SubjectStandingAtTime { get; set; }
    public float FootStrikeAtTime { get; set; }
    public float HeelPassingAtTime { get; set; }
    public float AngleDifferenceAtTime { get; set; }
    public float MMDistaceAtTime { get; set; }
    public float MaxAngleDifference { get; set; }
    public float MaxmmDistance { get; set; }
    public float KneeAbductionAtTime { get; set; }
    public float HipAbductionAtTime { get; set; }
    public float PelvisAngleAtTime { get; set; }
    public float AnkleAbductionAtTime { get; set; }
    public float VarusValgusAtTime { get; set; }
    public string SelectedLeg { get; set; }
    public string Condition { get; set; }
}
[System.Serializable]
public class HeelPressDetectionBody{
    public bool added { get; set; }
    public string Subject { get; set; }
    public string TimeOfHeelPressed{get; set; }
    public string nameOfTheFoot{ get; set; }
    public float distanceValue{ get; set; }
    public float angleDifferenceValue{ get; set; }
    public float kneeAbductionValue { get; set; }
    public float pelvisAngleValue{ get; set; }
    public float ankleAbductionValue { get; set; }
    public float varusValgusValue{ get; set; }
    
}

[System.Serializable]
public class StandingDetectionBody
{
    public bool added { get; set; }
    public string Subject { get; set; }
    public string TimeofStanding { get; set; }
    public string nameOfTheFoot { get; set; }
    public float distanceValue{ get; set; }
    public float angleDifferenceValue{ get; set; }
    public float kneeAbductionValue { get; set; }
    public float pelvisAngleValue{ get; set; }
    public float ankleAbductionValue { get; set; }
    public float varusValgusValue{ get; set; }
}

[System.Serializable]
public class AddClinicPatientBody
	{
        public string SubjectId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public long ClinicId { get; set; }
        public string AdminId { get; set; }
        public string DoctorId { get; set; }
        public string PatientId { get; set; }
    }
    
public class ShareRecordingRequest
{
    public string SharedBy { get; set; }
    public long RecordingId { get; set; }
    public string VideoURL { get; set; }
    public string UserEmail { get; set; }
}