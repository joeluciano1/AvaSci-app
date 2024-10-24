using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class AdminManager : MonoBehaviour
{
    public UserFromDB userFromDBPrefab;
    public RoleFromDb roleFromDbPrefab;
    public ForgetPassFromDB forgetPassFromDBPrefab;
    public SurveyFromDB surveyFromDBPrefab;

    public List<UserFromDB> GeneratedUsers = new List<UserFromDB>();
    public List<RoleFromDb> GeneratedRoles = new List<RoleFromDb>();
    public List<ForgetPassFromDB> GeneratedForgetPass = new List<ForgetPassFromDB>();
    public List<SurveyFromDB> GeneratedSurvey = new List<SurveyFromDB>();
    public void GetTables(bool value)
    {
        if (value)
        {
            ReferenceManager.instance.uiManager.AdminButton.gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.gray;
            APIHandler.instance.Get("General/GetTables", onSuccess: (response) =>
            {
                int count = 1;
                AdminResponse adminResponse = JsonConvert.DeserializeObject<AdminResponse>(response);
                if (adminResponse.isSuccess)
                {
                    Color bgColor = ReferenceManager.instance.uiManager.SelectedWindowColor;
                    Debug.Log("Users Count: "+adminResponse.result.users.Count);
                    foreach (var item in adminResponse.result.users)
                    {
                        if (GeneratedUsers.FirstOrDefault(x => x.Email.text.Equals(item.Email)) != null)
                        {
                            continue;
                        }
                        UserFromDB userFromDB = Instantiate(userFromDBPrefab, userFromDBPrefab.transform.parent);
                        if (bgColor == ReferenceManager.instance.uiManager.SelectedWindowColor)
                        {
                            bgColor = ReferenceManager.instance.uiManager.UnSelectedWindowColor;
                        }
                        else
                        {
                            bgColor = ReferenceManager.instance.uiManager.SelectedWindowColor;
                        }
                        userFromDB.BGImage.color = bgColor;
                        userFromDB.Username.text = count + ". " + item.Username;
                        count++;
                        userFromDB.Email.text = item.Email;
                        userFromDB.Country.text = item.CountryName;
                        userFromDB.Gender.text = item.Gender;
                        userFromDB.CreatedOn.text = item.Role;
                        userFromDB.gameObject.SetActive(true);
                        GeneratedUsers.Add(userFromDB);

                        SurveyFromDB surveyFromDB = Instantiate(surveyFromDBPrefab, surveyFromDBPrefab.transform.parent);
                        surveyFromDB.gameObject.SetActive(true);
                        surveyFromDB.Username.text = item.Username;
                        surveyFromDB.Reason.text = item.ReasonForDownload;
                        surveyFromDB.Interest.text = item.Interests.Replace(",", "\n");
                        GeneratedSurvey.Add(surveyFromDB);
                        if (!string.IsNullOrEmpty(item.AdvancedSurvey))
                        {
                            JointBody jointBody = null;
                            //item.AdvancedSurvey = item.AdvancedSurvey.Replace('\"', ' ');
                            try
                            {
                                jointBody = JsonConvert.DeserializeObject<JointBody>(item.AdvancedSurvey);
                            }
                            catch
                            {
                                continue;
                            }
                            if (jointBody == null)
                            {
                                continue;
                            }
                            for (int i = 0; i < jointBody.Joints.Count; i++)
                            {
                                if (i == 0)
                                {
                                    surveyFromDB.Advanced.text = $"<b>{jointBody.Joints[i].JointType.GetMeasurementTypeName()}</b>";
                                    if (jointBody.Joints[i].Healthy)
                                        surveyFromDB.Advanced.text += "\nWas a <b> healthy </b> joint when user signedup";
                                    if (jointBody.Joints[i].ExperiencePain)
                                        surveyFromDB.Advanced.text += "\nWas a <b> Experiencing Pain </b> in joint when user signedup";
                                    if (jointBody.Joints[i].ResultOfInjury)
                                        surveyFromDB.Advanced.text += "\nWas a <b> Result of Injury </b> in joint when user signedup";
                                    if (jointBody.Joints[i].PreviousSurgery)
                                        surveyFromDB.Advanced.text += "\nWas a <b> Previous Surgery </b> of joint when user signedup";
                                }
                            }
                            surveyFromDB.Advanced.text += $"\n <b>Others </b> \n {jointBody.Other}";
                            surveyFromDB.Advanced.text += $"\n <b>Fitness Level </b> \n {jointBody.LevelOfFitness}";

                        }
                    }
                    for (int i = 0; i < adminResponse.result.roles.Count; i++)
                    {
                        if (GeneratedRoles.FirstOrDefault(x => x.RoleName.text == adminResponse.result.roles[i]) != null)
                        {
                            continue;
                        }
                        if (bgColor == ReferenceManager.instance.uiManager.SelectedWindowColor)
                        {
                            bgColor = ReferenceManager.instance.uiManager.UnSelectedWindowColor;
                        }
                        else
                        {
                            bgColor = ReferenceManager.instance.uiManager.SelectedWindowColor;
                        }
                        RoleFromDb roleFromDb = Instantiate(roleFromDbPrefab, roleFromDbPrefab.transform.parent);
                        roleFromDb.gameObject.SetActive(true);
                        roleFromDb.RoleId.text = i.ToString();
                        roleFromDb.RoleName.text = adminResponse.result.roles[i];
                        roleFromDb.BGImage.color = bgColor;
                        GeneratedRoles.Add(roleFromDb);
                    }
                    foreach (var item in adminResponse.result.forgetPasses)
                    {
                        if (GeneratedForgetPass.FirstOrDefault(x => x.Email.text == item.UserEmail) != null)
                        {
                            continue;
                        }
                        ForgetPassFromDB forgetPassFromDB = Instantiate(forgetPassFromDBPrefab, forgetPassFromDBPrefab.transform.parent);
                        if (bgColor == ReferenceManager.instance.uiManager.SelectedWindowColor)
                        {
                            bgColor = ReferenceManager.instance.uiManager.UnSelectedWindowColor;
                        }
                        else
                        {
                            bgColor = ReferenceManager.instance.uiManager.SelectedWindowColor;
                        }
                        forgetPassFromDB.BGImage.color = bgColor;
                        forgetPassFromDB.Email.text = item.UserEmail;
                        forgetPassFromDB.Code.text = item.Code;
                        forgetPassFromDB.IsConsumed.text = item.IsConsumed ? "true" : "false";
                        forgetPassFromDB.CreatedOn.text = item.CreatedOn;
                        forgetPassFromDB.gameObject.SetActive(true);
                        GeneratedForgetPass.Add(forgetPassFromDB);
                    }
                }
                if (adminResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in adminResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show("Getting Reasons Failed!", $"Reasons are: {reasons}");
                    Debug.Log($"{adminResponse.serviceErrors}");
                }
            }, onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Getting Reasons Failed!", $"Reasons are: {error}");
                Debug.LogError($"Error: {error}");
            });
        }
        else
        {
            ReferenceManager.instance.uiManager.AdminButton.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        }
    }

}
