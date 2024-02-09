using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public GameObject LogoutButton;
    public GameObject AdminButton;
    public Color SelectedWindowColor;
    public Color UnSelectedWindowColor;

    public List<Button> ButtonImages;
    public List<GameObject> AdminPanels;

    public GameObject AvasciConsentPanel;

    public ScrollRect scrollRect;

    public Button AcceptButton;
    public TMP_Text ConsentText;

    public GameObject InstructionsPanel;
    public TMP_Text InstructionsText;
    public Image InstructionImage;
    int instructionsSiblingIndex;
    public TMP_Text UserInfotext;
    public void ClickButton(Button selectedButton)
    {
        ButtonImages.ForEach(x => x.image.color = UnSelectedWindowColor);
        selectedButton.image.color = SelectedWindowColor;
    }

    public void ClickSection(GameObject selectedPanel)
    {
        AdminPanels.ForEach(x => x.SetActive(false));
        selectedPanel.SetActive(true);
    }

    void Start()
    {
        instructionsSiblingIndex = InstructionsPanel.transform.GetSiblingIndex();
        if (PlayerPrefs.GetInt(StringConstants.FIRSTTIMEAPPRUN) == 0)
        {
            AvasciConsentPanel.SetActive(true);
            APIHandler.instance.Get("General/GetInstructions", onSuccess: (response) =>
            {
                InstructionsResponse instructionsResponse = JsonConvert.DeserializeObject<InstructionsResponse>(response);
                if (instructionsResponse.isSuccess)
                {
                    string instruction = instructionsResponse.result.FirstOrDefault(x => x.InstructionTitle.Contains("Consent")).Instruction;

                    instruction = instruction.Replace("\\n", "\n");

                    ConsentText.text = $"{instruction}";
                }
                if (instructionsResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in instructionsResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show("Signin Failed!", $"Reasons are: {reasons}");
                    Debug.Log($"{instructionsResponse.serviceErrors}");
                }
            },
            onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Signin Failed!", $"Reasons are: {error}");
                Debug.LogError($"Error: {error}");
            });
        }
    }
    IEnumerator DownloadImage(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            InstructionImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            InstructionImage.gameObject.SetActive(true);
            GeneralStaticManager.GlobalImage.Add("Instructions", InstructionImage.sprite);
        }
    }
    bool fetchedInstructions;
    public void GetInstructions(bool value)
    {
        if (value)
        {
            InstructionsPanel.transform.SetAsLastSibling();
            InstructionsPanel.SetActive(true);
            if (!fetchedInstructions)
            {
                APIHandler.instance.Get("General/GetInstructions", onSuccess: (response) =>
                          {
                              InstructionsResponse instructionsResponse = JsonConvert.DeserializeObject<InstructionsResponse>(response);
                              if (instructionsResponse.isSuccess)
                              {
                                  string instruction = instructionsResponse.result.FirstOrDefault(x => x.InstructionTitle.Contains("Instruction")).Instruction;
                                  instruction = instruction.Replace("\\n", "\n");
                                  string contentOfInstruction = instruction.Split('=')[0].Replace("image", "");
                                  string imageURL = instruction.Split('=')[1];
                                  StartCoroutine(DownloadImage(imageURL));
                                  InstructionsText.text = $"{contentOfInstruction}";
                                  fetchedInstructions = true;
                                  GeneralStaticManager.GlobalVar.Add("Instructions", contentOfInstruction);

                              }
                              if (instructionsResponse.isError)
                              {
                                  string reasons = "";
                                  foreach (var item in instructionsResponse.serviceErrors)
                                  {
                                      reasons += $"\n {item.code} {item.description}";
                                  }
                                  ReferenceManager.instance.PopupManager.Show("Signin Failed!", $"Reasons are: {reasons}");
                                  Debug.Log($"{instructionsResponse.serviceErrors}");
                              }
                          },
                          onError: (error) =>
                          {
                              ReferenceManager.instance.PopupManager.Show("Signin Failed!", $"Reasons are: {error}");
                              Debug.LogError($"Error: {error}");
                          });
            }
            else
            {
                InstructionsText.text = GeneralStaticManager.GlobalVar["Instructions"];
                if (GeneralStaticManager.GlobalImage.ContainsKey("Instructions"))
                    InstructionImage.sprite = GeneralStaticManager.GlobalImage["Instructions"];
            }
        }
        else
        {
            InstructionsPanel.transform.SetSiblingIndex(instructionsSiblingIndex);
            InstructionsPanel.SetActive(false);
        }
    }

    public void OnScroll()
    {

        if (scrollRect.verticalNormalizedPosition <= 0 && Input.GetMouseButton(0))
        {
            AcceptButton.interactable = true;
        }

    }
    public void DisplayUserInfo()
    {
        if (GeneralStaticManager.GlobalVar.ContainsKey("UserName"))
        {
            string username = GeneralStaticManager.GlobalVar["UserName"];
            string roles = GeneralStaticManager.GlobalVar["UserRoles"];
            string country = GeneralStaticManager.GlobalVar["UserCountry"];
            string gender = GeneralStaticManager.GlobalVar["UserGender"];
            string age = GeneralStaticManager.GlobalVar["UserAge"];
            UserInfotext.text = $"<b>User Name: </b> {username}\n \n<b>Roles: </b>{roles}\n \n<b>Country: </b>{country}\n \n<b>Gender: </b>{gender}\n \n<b>Age: </b>{age}";
        }
    }
    public void AcceptConsent()
    {
        PlayerPrefs.SetInt(StringConstants.FIRSTTIMEAPPRUN, 1);
        AvasciConsentPanel.SetActive(false);
    }
}
