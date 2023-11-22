using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System;

public class LoginManager : MonoBehaviour
{
    /// <summary>
    /// Signup
    /// </summary>
    public TMP_Dropdown Country_DropDown;
    public TMP_InputField Email_InputField;
    public TMP_InputField Password_InputField;
    public TMP_Dropdown Gender_DropDown;
    public TMP_InputField BirthDate_InputField;
    public FetchedInterests FetchedInterestsPrefab;
    public FetchedReasons FetchedReasonsPrefab;
    [HideInInspector]public List<FetchedInterests> SelectedFetchedInterests = new List<FetchedInterests>();
    [HideInInspector] public List<FetchedReasons> SelectedFetchedReasons = new List<FetchedReasons>();
    public TMP_InputField Other;
    public TMP_InputField LevelOfFitness;
    public List<JointMono> JointMonos = new List<JointMono>();
    public bool isAdvancedSurvey;
    public int selectedCountryId;
    /// <summary>
    /// Login
    /// </summary>
    public TMP_InputField LoginEmail_InputField;
    public TMP_InputField LoginPassword_InputField;

    private void Start()
    {
        if(string.IsNullOrWhiteSpace(PlayerPrefs.GetString(StringConstants.LOGINEMAIL)))
        {
            ReferenceManager.instance.SignupPanel.SetActive(true);
            ReferenceManager.instance.SigninPanel.SetActive(false);
        }
        else
        {
            ReferenceManager.instance.SignupPanel.SetActive(false);
            ReferenceManager.instance.SigninPanel.SetActive(false);
            LoginEmail_InputField.text = PlayerPrefs.GetString(StringConstants.LOGINEMAIL);
            LoginPassword_InputField.text = PlayerPrefs.GetString(StringConstants.LOGINPASSWORD);
            SignUserIn();
        }
    }

    public void SetIsAdvancedSurvey(bool value)
    {
        isAdvancedSurvey = value;
    }

    public void GetCountries()
    {
        ReferenceManager.instance.LoadingManager.ReasonLoading_Text.text = "Getting List of Countries";
        APIHandler.instance.Get("General/GetCountries",
            onSuccess: (response) =>
            {
                ProcessCountries(response);
            },
            onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Getting Countries Failed!", $"Reasons are: {error}");
                Debug.LogError($"Error: {error}");
            });
    }

    public void GetReasonsList()
    {
        APIHandler.instance.Get("General/GetReasons",
            onSuccess: (response) =>
            {
                GetReasonResponse reasonResponse = JsonConvert.DeserializeObject<GetReasonResponse>(response);
                if (reasonResponse.isSuccess)
                {
                    foreach (var item in reasonResponse.result)
                    {
                        FetchedReasons fetchedReasons = Instantiate(FetchedReasonsPrefab, FetchedReasonsPrefab.transform.parent);
                        item.name = GeneralStaticManager.AddSpacesToSentence(item.name, true);
                        fetchedReasons.Id = item.id;
                        fetchedReasons.gameObject.SetActive(true);
                        fetchedReasons.name = item.name;
                        fetchedReasons.ItemName.text = item.name;
                    }
                }
                if (reasonResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in reasonResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show("Getting Reasons Failed!", $"Reasons are: {reasons}");
                    Debug.Log($"{reasonResponse.serviceErrors}");
                }
            },
            onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Getting Reasons Failed!", $"Reasons are: {error}");
                Debug.LogError($"Error: {error}");
            });
    }

    public void GetInterestsList()
    {
        ReferenceManager.instance.LoadingManager.ReasonLoading_Text.text = "Getting List of Interests Questions";
        APIHandler.instance.Get("General/GetInterests",
            onSuccess: (response) =>
            {
                InterestsResponse interestsResponse = JsonConvert.DeserializeObject<InterestsResponse>(response);
                if (interestsResponse.isSuccess)
                {
                    foreach(var item in interestsResponse.result)
                    {
                        FetchedInterests fetchedInterests = Instantiate(FetchedInterestsPrefab, FetchedInterestsPrefab.transform.parent);
                        item.name = GeneralStaticManager.AddSpacesToSentence(item.name, true);
                        fetchedInterests.Id = item.id;
                        fetchedInterests.gameObject.SetActive(true);
                        fetchedInterests.name = item.name;
                        fetchedInterests.ItemName.text = item.name;
                    }
                }
                if (interestsResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in interestsResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show("Getting Interests Questions Failed!", $"Reasons are: {reasons}");
                    Debug.Log($"{interestsResponse.serviceErrors}");
                }
            },
            onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Getting Interests Questions Failed!", $"Reasons are: {error}");
                Debug.LogError($"Error: {error}");
            });
    }

    private void ProcessCountries(string response)
    {
        CountryResponse countryResponse = JsonConvert.DeserializeObject<CountryResponse>(response);
        if (countryResponse.isSuccess)
        {
            List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>();
            foreach (CountryData country in countryResponse.result)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData()
                {
                    text = $"{country.name}"
                };

                optionDatas.Add(optionData);
            }
            PlayerPrefs.SetString(StringConstants.COUNTRYRESPONSE, response);
            Country_DropDown.AddOptions(optionDatas);
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(StringConstants.COUNTRYRESPONSE)))
                ReferenceManager.instance.PopupManager.Show("Success!", "Countries Fetched");

        }
        if (countryResponse.isError)
        {
            string reasons = "";
            foreach (var item in countryResponse.serviceErrors)
            {
                reasons += $"\n {item.code} {item.description}";
            }
            ReferenceManager.instance.PopupManager.Show("Signup Failed!", $"Reasons are: {reasons}");
            Debug.Log($"{countryResponse.serviceErrors}");
        }
        Debug.Log($"Success: {response}");
    }

    public void SignUserUp()
    {
        ReferenceManager.instance.LoadingManager.ReasonLoading_Text.text = "Signing You Up!";
        string interestIds = "";
        foreach(var item in SelectedFetchedInterests)
        {
            if (string.IsNullOrWhiteSpace(interestIds))
            {
                interestIds = item.Id.ToString();
            }
            else
            {
                interestIds += "," + item.Id.ToString();
            }
        }

        string reasonsIds = "";
        foreach (var item in SelectedFetchedReasons)
        {
            if (string.IsNullOrWhiteSpace(reasonsIds))
            {
                reasonsIds = item.Id.ToString();
            }
            else
            {
                reasonsIds += "," + item.Id.ToString();
            }
        }

        string advancedSurveyJson = "";
        if (isAdvancedSurvey)
        {
            JointBody jointBody = new JointBody();
            foreach(var item in JointMonos)
            {
                JointData joint = new JointData()
                {
                    JointType = item.JointType,
                    ExperiencePain = item.ExperiencePain,
                    Healthy = item.Healthy,
                    PreviousSurgery = item.PreviousSurgery,
                    ResultOfInjury = item.ResultOfInjury
                };
                jointBody.Joints.Add(joint);
                jointBody.Other = Other.text;
                jointBody.LevelOfFitness = LevelOfFitness.text;
            }
            advancedSurveyJson = JsonConvert.SerializeObject(jointBody);
        }
            
        
        SignupBody signupBody = new SignupBody()
        {
            countryId = selectedCountryId,
            email = Email_InputField.text,
            password = Password_InputField.text,
            genderId = Gender_DropDown.value,
            birthDate = DateTime.Parse(BirthDate_InputField.text + "-01-01"),
            reasonForDownload = reasonsIds,
            interests = interestIds,
            AdvancedSurvey = isAdvancedSurvey? advancedSurveyJson:null
        };
        string jsonData = JsonConvert.SerializeObject(signupBody);
        APIHandler.instance.Post("Auth/Signup",jsonData,
           onSuccess: (response) =>
           {
             SignupResponse signupResponse = JsonConvert.DeserializeObject<SignupResponse>(response);
               if (signupResponse.isSuccess)
               {
                   ReferenceManager.instance.PopupManager.Show("Success!", "You have successfully signedup");
               }
               if (signupResponse.isError)
               {
                   string reasons = "";
                   foreach(var item in signupResponse.serviceErrors)
                   {
                       reasons += $"\n {item.code} {item.description}";
                   }
                   ReferenceManager.instance.PopupManager.Show("Signup Failed!", $"Reasons are: {reasons}");
                   Debug.Log($"{signupResponse.serviceErrors}");
               }
               ReferenceManager.instance.SignupPanel.SetActive(false);
               ReferenceManager.instance.SigninPanel.SetActive(true);
               ClearAllFields();
               Debug.Log($"Success: {response}");

           },
           onError: (error) =>
           {
               ReferenceManager.instance.PopupManager.Show("Signup Failed!", $"Reasons are: {error}");
               Debug.LogError($"Error: {error}");
           });

    }

    public void SignUserIn()
    {
        ReferenceManager.instance.LoadingManager.ReasonLoading_Text.text = "Signing You In!";
        LoginBody loginBody = new LoginBody()
        {
            email = LoginEmail_InputField.text,
            password = LoginPassword_InputField.text
        };

        string jsonData = JsonConvert.SerializeObject(loginBody);

        APIHandler.instance.Post("Auth/Login", jsonData,
          onSuccess: (response) =>
          {
              SignInResponse signinResponse = JsonConvert.DeserializeObject<SignInResponse>(response);
              if (signinResponse.isSuccess)
              {
                  ReferenceManager.instance.PopupManager.Show("Success!", "You have successfully signed in");
                  StringConstants.TOKEN = signinResponse.result.token;
                  GeneralStaticManager.GlobalVar.Add("UserName", signinResponse.result.UserName);
                  ReferenceManager.instance.SigninPanel.SetActive(false);
                  PlayerPrefs.SetString(StringConstants.LOGINEMAIL, LoginEmail_InputField.text);
                  PlayerPrefs.SetString(StringConstants.LOGINPASSWORD, LoginPassword_InputField.text);
                  ReferenceManager.instance.UsernameText.text = signinResponse.result.UserName;
                  ReferenceManager.instance.Screen1.SetActive(true);
              }
              if (signinResponse.isError)
              {
                  string reasons = "";
                  foreach (var item in signinResponse.serviceErrors)
                  {
                      reasons += $"\n {item.code} {item.description}";
                  }
                  ReferenceManager.instance.PopupManager.Show("Signin Failed!", $"Reasons are: {reasons}");
                  Debug.Log($"{signinResponse.serviceErrors}");
              }
              
              Debug.Log($"Success: {response}");

          },
          onError: (error) =>
          {
              ReferenceManager.instance.PopupManager.Show("Signin Failed!", $"Reasons are: {error}");
              Debug.LogError($"Error: {error}");
          });
    }

   public void ClearAllFields()
    {
        Email_InputField.text = string.Empty;
        Password_InputField.text = string.Empty;
        BirthDate_InputField.text = string.Empty;
    }

    
}
