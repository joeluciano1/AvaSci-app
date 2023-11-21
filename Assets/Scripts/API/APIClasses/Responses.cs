using System.Collections.Generic;

public class Responses
{
   
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

