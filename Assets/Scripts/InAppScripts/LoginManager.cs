using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Nrjwolf.Tools;

public class LoginManager : MonoBehaviour
{
    /// <summary>
    /// Signup
    /// </summary>
    public TMP_Dropdown Country_DropDown;
    public TMP_InputField Email_InputField;
    public TMP_InputField UserName_InputField;
    public TMP_InputField Password_InputField;
    public TMP_Dropdown Gender_DropDown;
    public TMP_InputField BirthDate_InputField;
    public FetchedInterests FetchedInterestsPrefab;
    public FetchedReasons FetchedReasonsPrefab;
    [HideInInspector] public List<FetchedInterests> SelectedFetchedInterests = new List<FetchedInterests>();
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
        Application.runInBackground = true;
        if (string.IsNullOrWhiteSpace(PlayerPrefs.GetString(StringConstants.LOGINEMAIL)))
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
    #region Login And Signup
    public void Logout()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(StringConstants.FIRSTTIMEAPPRUN, 1);
        ReferenceManager.instance.LoadingManager.Show("Logging You Out");
        GeneralStaticManager.GlobalVar.Clear();
        GeneralStaticManager.GraphsReadings.Clear();
        SceneManager.LoadSceneAsync("Main");
    }
    public void SetIsAdvancedSurvey(bool value)
    {
        isAdvancedSurvey = value;
    }
    public void FetchSequentiallyV()
    {
        StartCoroutine(FetchSequencially());
    }
    public IEnumerator FetchSequencially()
    {
        if (!GeneralStaticManager.GlobalVar.ContainsKey(StringConstants.COUNTRYRESPONSE))
        {
            GetCountries();
            yield return new WaitUntil(() => GeneralStaticManager.GlobalVar.ContainsKey(StringConstants.COUNTRYRESPONSE));
        }
        if (!GeneralStaticManager.GlobalVar.ContainsKey(StringConstants.REASONSRESPONSE))
        {
            GetReasonsList();
            yield return new WaitUntil(() => GeneralStaticManager.GlobalVar.ContainsKey(StringConstants.REASONSRESPONSE));
        }
        if (!GeneralStaticManager.GlobalVar.ContainsKey(StringConstants.INTERESTSRESPONSE))
        {
            GetInterestsList();
            yield return new WaitUntil(() => GeneralStaticManager.GlobalVar.ContainsKey(StringConstants.INTERESTSRESPONSE));
        }
    }

    public void GetCountries()
    {
        if (GeneralStaticManager.GlobalVar.ContainsKey(StringConstants.COUNTRYRESPONSE))
        {
            return;
        }
        ReferenceManager.instance.LoadingManager.ReasonLoading_Text.text = "Getting List of Countries";
        APIHandler.instance.Get("General/GetCountries",
            onSuccess: (response) =>
            {
                ProcessCountries(response);
            },
            onError: (error) =>
            {
                IOSNativeAlert.ShowAlertMessage("Getting Countries Failed!", $"Reasons are: {error}");

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
                    GeneralStaticManager.GlobalVar.Add(StringConstants.REASONSRESPONSE, response);
                }
                if (reasonResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in reasonResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    IOSNativeAlert.ShowAlertMessage("Getting Reasons Failed!", $"Reasons are: {reasons}");
                    Debug.Log($"{reasonResponse.serviceErrors}");
                }
            },
            onError: (error) =>
            {
                IOSNativeAlert.ShowAlertMessage("Getting Reasons Failed!", $"Reasons are: {error}");
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
                    foreach (var item in interestsResponse.result)
                    {
                        FetchedInterests fetchedInterests = Instantiate(FetchedInterestsPrefab, FetchedInterestsPrefab.transform.parent);
                        item.name = GeneralStaticManager.AddSpacesToSentence(item.name, true);
                        fetchedInterests.Id = item.id;
                        fetchedInterests.gameObject.SetActive(true);
                        fetchedInterests.name = item.name;
                        fetchedInterests.ItemName.text = item.name;
                    }
                    GeneralStaticManager.GlobalVar.Add(StringConstants.INTERESTSRESPONSE, response);
                }
                if (interestsResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in interestsResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    IOSNativeAlert.ShowAlertMessage("Getting Interests Questions Failed!", $"Reasons are: {reasons}");
                    Debug.Log($"{interestsResponse.serviceErrors}");
                }
            },
            onError: (error) =>
            {
                IOSNativeAlert.ShowAlertMessage("Getting Interests Questions Failed!", $"Reasons are: {error}");
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
            GeneralStaticManager.GlobalVar.Add(StringConstants.COUNTRYRESPONSE, response);
            Country_DropDown.AddOptions(optionDatas);
            IOSNativeAlert.ShowAlertMessage("Success!", "Countries Fetched", new IOSNativeAlert.AlertButton("ok", () => { Debug.Log("Chall Gya"); }));

        }
        if (countryResponse.isError)
        {
            string reasons = "";
            foreach (var item in countryResponse.serviceErrors)
            {
                reasons += $"\n {item.code} {item.description}";
            }
            IOSNativeAlert.ShowAlertMessage("Signup Failed!", $"Reasons are: {reasons}");
            Debug.Log($"{countryResponse.serviceErrors}");
        }
        Debug.Log($"Success: {response}");
    }

    public void SignUserUp()
    {
        ReferenceManager.instance.LoadingManager.ReasonLoading_Text.text = "Signing You Up!";
        string interestIds = "";
        foreach (var item in SelectedFetchedInterests)
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
            foreach (var item in JointMonos)
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
            userName = UserName_InputField.text,
            password = Password_InputField.text,
            genderId = Gender_DropDown.value,
            birthDate = DateTime.Parse(BirthDate_InputField.text + "-01-01"),
            reasonForDownload = reasonsIds,
            interests = interestIds,
            AdvancedSurvey = isAdvancedSurvey ? advancedSurveyJson : null
        };
        string jsonData = JsonConvert.SerializeObject(signupBody);
        APIHandler.instance.Post("Auth/Signup", jsonData,
           onSuccess: (response) =>
           {
               SignupResponse signupResponse = JsonConvert.DeserializeObject<SignupResponse>(response);
               if (signupResponse.isSuccess)
               {
                   IOSNativeAlert.ShowAlertMessage("Success!", "You have successfully signedup");
               }
               if (signupResponse.isError)
               {
                   string reasons = "";
                   foreach (var item in signupResponse.serviceErrors)
                   {
                       reasons += $"\n {item.code} {item.description}";
                   }
                   IOSNativeAlert.ShowAlertMessage("Signup Failed!", $"Reasons are: {reasons}");
                   Debug.Log($"{signupResponse.serviceErrors}");
               }
               ReferenceManager.instance.SignupPanel.SetActive(false);
               ReferenceManager.instance.SigninPanel.SetActive(true);
               ClearAllFields();
               Debug.Log($"Success: {response}");

           },
           onError: (error) =>
           {
               IOSNativeAlert.ShowAlertMessage("Signup Failed!", $"Reasons are: {error}");
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
                  if (signinResponse.result.IsSubscribed && ReferenceManager.instance.iAPManager.m_StoreController != null && ReferenceManager.instance.iAPManager.m_StoreController.products.all.Length != 0)
                  {
                      if (!ReferenceManager.instance.iAPManager.CheckSubscription("avascimonthlysub", signinResponse.result.Receipt) && !ReferenceManager.instance.iAPManager.CheckSubscription("avasciyearlysub", signinResponse.result.Receipt))
                      {
                          IOSNativeAlert.ShowAlertMessage("Signin Failed!", $"Your Subscription has Ended and you need to subscribe", new IOSNativeAlert.AlertButton("ok", () => { ReferenceManager.instance.IAPPAnel.SetActive(true); }));
                          return;
                      }
                  }
                  IOSNativeAlert.ShowAlertMessage("Success!", $"You have successfully signed in\n{signinResponse.result.SpecialMessage}");
                  StringConstants.TOKEN = signinResponse.result.token;
                  GeneralStaticManager.GlobalVar.Add("UserName", signinResponse.result.UserName);
                  GeneralStaticManager.GlobalVar.Add("UserRoles", string.Join(',', signinResponse.result.Roles));
                  GeneralStaticManager.GlobalVar.Add("UserAge", signinResponse.result.Age);
                  GeneralStaticManager.GlobalVar.Add("UserCountry", signinResponse.result.CountryName);
                  GeneralStaticManager.GlobalVar.Add("UserGender", signinResponse.result.Gender);
                  GeneralStaticManager.GlobalVar.Add("UserID", signinResponse.result.UserId);
                  ReferenceManager.instance.SigninPanel.SetActive(false);
                  PlayerPrefs.SetString(StringConstants.LOGINEMAIL, LoginEmail_InputField.text);
                  PlayerPrefs.SetString(StringConstants.LOGINPASSWORD, LoginPassword_InputField.text);
                  ReferenceManager.instance.UsernameText.text = signinResponse.result.UserName;
                  ReferenceManager.instance.Screen1.SetActive(true);
                  ReferenceManager.instance.uiManager.LogoutButton.SetActive(true);
                  if (signinResponse.result.Roles.Contains("SuperUser"))
                  {
                      ReferenceManager.instance.uiManager.AdminButton.SetActive(true);
                  }
                  else
                  {
                      ReferenceManager.instance.uiManager.AdminButton.SetActive(false);
                  }
              }
              if (signinResponse.isError)
              {
                  string reasons = "";
                  bool isSubscriptionEnded = false;
                  foreach (var item in signinResponse.serviceErrors)
                  {
                      reasons += $"\n {item.code} {item.description}";
                      if (item.code == "E0020")
                      {
                          isSubscriptionEnded = true;
                      }
                  }
                  if (isSubscriptionEnded)
                  {
                      IOSNativeAlert.ShowAlertMessage("Signin Failed!", $"Your Trial has Ended and you need to subscribe", new IOSNativeAlert.AlertButton("ok", () => { ReferenceManager.instance.IAPPAnel.SetActive(true); }));
                  }
                  else
                  {
                      IOSNativeAlert.ShowAlertMessage("Signin Failed!", $"Reasons are: {reasons}");
                  }
                  Debug.Log($"{signinResponse.serviceErrors}");
              }

              Debug.Log($"Success: {response}");




          },
          onError: (error) =>
          {
              IOSNativeAlert.ShowAlertMessage("Signin Failed!", $"Reasons are: {error}");
              Debug.LogError($"Error: {error}");
          });


    }

    public void ClearAllFields()
    {
        Email_InputField.text = string.Empty;
        Password_InputField.text = string.Empty;
        BirthDate_InputField.text = string.Empty;
    }
    #endregion
    public void TriggerEmailOnBackend(TMP_InputField emailInput)
    {
        ReferenceManager.instance.forgetPasswordManager.EmailRequested = emailInput.text;
        SendEmailRequest request = new SendEmailRequest()
        {
            ToEmail = emailInput.text,
            Subject = "ForgotPassword",
            Message = ""
        };
        string json = JsonConvert.SerializeObject(request);
        Debug.Log(json);
        APIHandler.instance.Post("Auth/ForgetPassword", json,
        onSuccess: (response) =>
        {
            ResponseWithNoObject sendEmailResponse = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
            if (sendEmailResponse.isSuccess)
            {
                ReferenceManager.instance.forgetPasswordManager.EnterCodeSection.SetActive(true);
            }
            if (sendEmailResponse.isError)
            {
                string reasons = "";
                foreach (var item in sendEmailResponse.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                IOSNativeAlert.ShowAlertMessage("Signin Failed!", $"Reasons are: {reasons}");
                Debug.Log($"{sendEmailResponse.serviceErrors}");
            }

        },
        onError: (error) =>
        {
            IOSNativeAlert.ShowAlertMessage("Email Sending Failed!", $"Reasons are: {error}");
            Debug.LogError($"Error: {error}");
        }
        );
    }

    public void ValidateCodeFromBackend(TMP_InputField codeInput)
    {
        CheckCodeBody request = new CheckCodeBody()
        {
            Email = ReferenceManager.instance.forgetPasswordManager.EmailRequested,
            Code = codeInput.text
        };
        string json = JsonConvert.SerializeObject(request);

        APIHandler.instance.Post("Auth/CheckCode", json,
        onSuccess: (response) =>
        {
            ResponseWithNoObject codeValidateReponse = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
            if (codeValidateReponse.isSuccess)
            {
                ReferenceManager.instance.forgetPasswordManager.EnterNewPasswordSecion.SetActive(true);
            }
            if (codeValidateReponse.isError)
            {
                string reasons = "";
                foreach (var item in codeValidateReponse.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                IOSNativeAlert.ShowAlertMessage("Signin Failed!", $"Reasons are: {reasons}");
                Debug.Log($"{codeValidateReponse.serviceErrors}");
            }

        },
        onError: (error) =>
        {
            IOSNativeAlert.ShowAlertMessage("Email Sending Failed!", $"Reasons are: {error}");
            Debug.LogError($"Error: {error}");
        }
        );
    }
    public void ResetPassword()
    {
        string newPassword = ReferenceManager.instance.forgetPasswordManager.NewPasswordField.text;
        string confirmPassword = ReferenceManager.instance.forgetPasswordManager.ConfirmPasswordField.text;

        if (!newPassword.Equals(confirmPassword) || string.IsNullOrEmpty(newPassword))
        {
            IOSNativeAlert.ShowAlertMessage("Issue!", "Passwords DontMatch or cant be empty please try again");
            return;
        }

        ResetPasswordBody request = new ResetPasswordBody()
        {
            Email = ReferenceManager.instance.forgetPasswordManager.EmailRequested,
            NewPassword = newPassword
        };
        string json = JsonConvert.SerializeObject(request);

        APIHandler.instance.Post("Auth/ResetPassword", json,
        onSuccess: async (response) =>
        {
            ResponseWithNoObject codeValidateReponse = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
            if (codeValidateReponse.isSuccess)
            {
                IOSNativeAlert.ShowAlertMessage("Password is Reset!", "Your Password Is Reset Successfully", new IOSNativeAlert.AlertButton("Thank You", () => { ReferenceManager.instance.forgetPasswordManager.gameObject.SetActive(false); }));
            }
            if (codeValidateReponse.isError)
            {
                string reasons = "";
                foreach (var item in codeValidateReponse.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                IOSNativeAlert.ShowAlertMessage("Signin Failed!", $"Reasons are: {reasons}");
                Debug.Log($"{codeValidateReponse.serviceErrors}");
            }

        },
        onError: (error) =>
        {
            IOSNativeAlert.ShowAlertMessage("Email Sending Failed!", $"Reasons are: {error}");
            Debug.LogError($"Error: {error}");
        }
        );
    }
    public void DeleteAccount()
    {
        DeleteAccountBody deleteAccountBody = new DeleteAccountBody()
        {
            UserID = GeneralStaticManager.GlobalVar["UserID"]
        };
        string json = JsonConvert.SerializeObject(deleteAccountBody);
        APIHandler.instance.Post("Auth/DeleteAccount", json, onSuccess: (response) =>
        {
            ResponseWithNoObject deleteAccountResponse = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
            if (deleteAccountResponse.isSuccess)
            {
                IOSNativeAlert.ShowAlertMessage("Account Deleted!", "Your Account Is Deleted Successfully", new IOSNativeAlert.AlertButton("Thank You", () => { Logout(); }));
            }
            if (deleteAccountResponse.isError)
            {
                string reasons = "";
                foreach (var item in deleteAccountResponse.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                IOSNativeAlert.ShowAlertMessage("Account Deletion Failed!", $"Reasons are: {reasons}");
                Debug.Log($"{deleteAccountResponse.serviceErrors}");
            }

        }, onError: (error) =>
        {
            IOSNativeAlert.ShowAlertMessage("Account Deletion Failed!", $"Reasons are: {error}");
        });
    }

}
