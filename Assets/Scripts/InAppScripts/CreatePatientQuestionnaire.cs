using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json;
using Nrjwolf.Tools;
public class CreatePatientQuestionnaire : MonoBehaviour
{
    public TMP_InputField EmailInputField;
    public TMP_InputField SubjectNumberInputField;
    public TMP_Dropdown ClinicsDropDown;
    public TMP_Dropdown DoctorsDropDown;
    public Button DoneButton;

    public void CreatePatient()
    {
        var selectedClinic = ReferenceManager.instance.LoginManager.signinResponse.result.clinics.FirstOrDefault(x => x.ClinicName == ClinicsDropDown.captionText.text);
        var selectedDoctor = ReferenceManager.instance.LoginManager.signinResponse.result.doctors.FirstOrDefault(x => x.DoctorName == DoctorsDropDown.captionText.text);
        AddClinicPatientBody addClinicPatientBody = new()
        {
            Email = EmailInputField.text,
            SubjectId = SubjectNumberInputField.text,
            ClinicId = selectedClinic.ClinicId,
            DoctorId = selectedDoctor.DoctorId
        };
        Debug.Log("clinicID is: " + selectedClinic.ClinicId);
        string json = JsonConvert.SerializeObject(addClinicPatientBody);

         APIHandler.instance.Post("General/AddClinicPatient", json,
        onSuccess: (response) =>
        {
            Debug.Log(response);
            GenericStringResponse genericResponse = JsonConvert.DeserializeObject<GenericStringResponse>(response);
            if(genericResponse.isOk){
                ReferenceManager.instance.commentQuestionnaire.PatientsDropDown.options.Add(new TMP_Dropdown.OptionData(EmailInputField.text.Split('@')[0]));
            }
            else{
                IOSNativeAlert.ShowAlertMessage("Failed!", $"UnknownError");
            }

        },
        onError: (error) =>
        {
            IOSNativeAlert.ShowAlertMessage("Failed!", $"Reasons are: {error}");
            Debug.LogError($"Error: {error}");
        }
        );
    }
    private void Start()
    {
        UpdateAccoridngToSelectedClinic();
    }
    public void UpdateAccoridngToSelectedClinic()
    {
        DoctorsDropDown.options.Clear();
        var selectedClinic = ReferenceManager.instance.LoginManager.signinResponse.result.clinics.FirstOrDefault(x => x.ClinicName == ClinicsDropDown.captionText.text);
        var doctorsInSelectedClinic = ReferenceManager.instance.LoginManager.signinResponse.result.doctors.Where(x => x.DoctorClinicId == selectedClinic.ClinicId).ToList();
        foreach(var doctor in doctorsInSelectedClinic){
            DoctorsDropDown.options.Add(new TMP_Dropdown.OptionData(doctor.DoctorName));
        }
    }
}
