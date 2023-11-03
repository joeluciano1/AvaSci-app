using System;

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
}

public class LoginBody
{
    public string email { get; set; }
    public string password { get; set; }
}

