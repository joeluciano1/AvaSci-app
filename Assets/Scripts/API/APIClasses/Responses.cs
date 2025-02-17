using System.Collections.Generic;

public class Responses
{

}

public class RReportResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public BaseRResponse result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class BaseRResponse
{
    public string HtmlResponse { get; set; }
}
public class CountryData
{
    public int id { get; set; }
    public string name { get; set; }
    public string iso { get; set; }
    public string isO2 { get; set; }
    public string icon { get; set; }
}

public class CountryResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public List<CountryData> result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class ServiceError
{
    public string code { get; set; }
    public string description { get; set; }
}


public class SignupResult
{
    public string message { get; set; }
}

public class SignupResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public SignupResult result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class Interest
{
    public int id { get; set; }
    public string name { get; set; }
}

public class InterestsResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public List<Interest> result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class SignInResult
{
    public string token { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; }
    public string CountryName { get; set; }
    public string Age { get; set; }
    public string Gender { get; set; }
    public string SpecialMessage { get; set; }
    public string UserId { get; set; }
    public bool IsSubscribed { get; set; }
    public string Receipt { get; set; }
    public string SubjectID { get; set; }
    public List<Patient> patients { get; set; }
    public List<Clinic> clinics { get; set; }
	public List<Doctor> doctors { get; set; }
}
public class Patient
	{
		public string PatientId { get; set; }
		public string SubjectId { get; set; }
        public string PatientName { get; set; }
        public long ClinicId { get; set; }
		public string ClinicName { get; set; }
		public string DoctorName { get; set; }
	}
public class Clinic
	{
		public string ClinicName { get; set; }
		public long ClinicId { get; set; }
	}

	public class Doctor
	{
		public string DoctorName { get; set; }
		public string DoctorId { get; set; }
        public long DoctorClinicId { get; set; }
	}
public class SignInResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public SignInResult result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class GetReasonResult
{
    public int id { get; set; }
    public string name { get; set; }
}

public class GetReasonResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public List<GetReasonResult> result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class ResponseWithNoObject
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class AdminResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public AdminResponseData result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class AdminResponseData
{
    public AdminResponseData()
    {
        users = new List<User>();
        roles = new List<string>();
        forgetPasses = new List<ForgetPass>();
    }
    public List<User> users { get; set; }
    public List<string> roles { get; set; }
    public List<ForgetPass> forgetPasses { get; set; }
}
public class User
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string CountryName { get; set; }
    public string ReasonForDownload { get; set; }
    public string Gender { get; set; }
    public string DateCreated { get; set; }
    public string Interests { get; set; }
    public string AdvancedSurvey { get; set; }
    public string Role { get; set; }
}

public class ForgetPass
{
    public string Code { get; set; }
    public bool IsConsumed { get; set; }
    public string CreatedOn { get; set; }
    public string UserEmail { get; set; }
}

public class InstructionsResponseData
{
    public string InstructionTitle { get; set; }
    public string Instruction { get; set; }
}
public class InstructionsResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public List<InstructionsResponseData> result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}

public class UserReportData
{
    public long Id { get; set; }
    public string UserID { get; set; }
    public string UserName { get; set; }
    public string VideoURL { get; set; }
    public string ReportURL { get; set; }
    public string CreatedOn { get; set; }
    public string ReportDescription { get; set; }
    public string GroupName { get; set; }
    public string SubjectId { get; set; }
    public bool HasHtmlReports { get; set; }
    public List<JointReading> JointReadings { get; set; } = new List<JointReading>();
    public List<TimeBasedReadingRequest> TimeBasedReadings { get; set; } = new List<TimeBasedReadingRequest>();
    public List<GetGaitReportResponse> GaitReports { get; set; } = new List<GetGaitReportResponse>();
}
[System.Serializable]
public class GetGaitReportResponse
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

    // public float KneeAbductionAtTime { get; set; }
    public float HipAbductionAtTime { get; set; }
    public float PelvisAngleAtTime { get; set; }
    public float AnkleAbductionAtTime { get; set; }
    public float VarusValgusAtTime { get; set; }
    public string SelectedLeg { get; set; }
    public string VideoName { get; set; }
}
public class UserReportResponse
{
    public bool isSuccess { get; set; }
    public bool isError { get; set; }
    public int status { get; set; }
    public List<UserReportData> result { get; set; }
    public List<ServiceError> serviceErrors { get; set; }
}
public class Receipt
{
    public string Payload { get; set; }
    public string Store { get; set; }
    public string TransactionID { get; set; }
}

 public class GenericStringResponse
    {
        public object result { get; set; }
        public bool isOk { get; set; }
        public bool isError { get; set; }
        public int status { get; set; }
        
    }

 public class HtmlBaseResponse
 {
     public bool isSuccess { get; set; }
     public bool isError { get; set; }
     public int status { get; set; }
     public HtmlReportResponse result { get; set; }
     public List<ServiceError> serviceErrors { get; set; }
 }
 public class HtmlReportResponse
 {

     public List<HtmlContents> htmlContents { get; set; } = new List<HtmlContents>();
 }
 public class HtmlContents
 {
     public string CreatedBy { get; set; }

     public string HtmlReport { get; set; }
     public string CreatedOn { get; set; }
 }