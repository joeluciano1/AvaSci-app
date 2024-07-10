using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringConstants
{
    // public const string BASEENDPOINT = "http://localhost:5278/api/v1";
    public const string BASEENDPOINT = "https://avasci-backend.azurewebsites.net/api/v1";
    public const string COUNTRYRESPONSE = "CountryResponse";
    public const string REASONSRESPONSE = "ReasonsResponse";
    public const string INTERESTSRESPONSE = "InteresetsResponse";
    public const string FIRSTTIMEAPPRUN = "FirstTimeRun";
    public static string TOKEN;

    /// <summary>
    /// PlayerPrefs
    /// </summary>
    public const string LOGINEMAIL = "LoginEmail";
    public const string LOGINPASSWORD = "LoginPass";
    public const float ANIMATIONTIME = 0.5f;
}
