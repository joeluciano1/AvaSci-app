using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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
    bool fetchedInstructions;
    public void GetInstructions(bool value)
    {
        if (value)
        {
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
                                  InstructionsText.text = $"{instruction}";
                                  fetchedInstructions = true;
                                  GeneralStaticManager.GlobalVar.Add("Instructions", instruction);
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
            }
        }
        else
        {
            InstructionsPanel.SetActive(false);
        }
    }

    public void OnScroll()
    {
        Debug.Log("alskdjflsajflisajflsajdfsdf: " + scrollRect.verticalNormalizedPosition);
        if (scrollRect.verticalNormalizedPosition <= 0 && Input.GetMouseButton(0))
        {
            AcceptButton.interactable = true;
        }

    }

    public void AcceptConsent()
    {
        PlayerPrefs.SetInt(StringConstants.FIRSTTIMEAPPRUN, 1);
        AvasciConsentPanel.SetActive(false);
    }
}
