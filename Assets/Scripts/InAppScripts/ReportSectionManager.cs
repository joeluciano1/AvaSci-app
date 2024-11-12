using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReportSectionManager : MonoBehaviour
{
    public Color selectedColor;
    public Color nonSelectedColor;
    public Button ReportsButton;
    public Button ClinicsButton;

    public GameObject ReportsSection;
    public GameObject ClinicsSection;
    public GameObject HeadingReports;
    public ClinicDataFromDB clinicDataFromDB;
    bool clinicDataFilled;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClinicsButton.onClick.RemoveAllListeners();
        ReportsButton.onClick.RemoveAllListeners();

        ClinicsButton.onClick.AddListener(() => OpenClinic());
        ReportsButton.onClick.AddListener(()=> OpenReports());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OpenClinic()
    {
        ReportsButton.targetGraphic.color = nonSelectedColor;
        ClinicsButton.targetGraphic.color = selectedColor;

        ReportsSection.SetActive(false);
        ClinicsSection.SetActive(true);
        HeadingReports.SetActive(false);

        if(!clinicDataFilled)
        FeedClinicData();
    }
    public void OpenReports()
    {
        ReportsButton.targetGraphic.color = selectedColor;
        ClinicsButton.targetGraphic.color = nonSelectedColor;

        ReportsSection.SetActive(true);
        ClinicsSection.SetActive(false);
        HeadingReports.SetActive(true);
    }
    List<ClinicDataFromDB> clinicDataFromDBStored = new List<ClinicDataFromDB>();
    public void FeedClinicData()
    {
        if(clinicDataFromDBStored.Count !=0)
        {
            clinicDataFromDBStored.ForEach(x => Destroy(x.gameObject));
            clinicDataFromDBStored.Clear();
        }
        clinicDataFilled = true;
        foreach(var item in ReferenceManager.instance.LoginManager.signinResponse.result.clinics)
        {
            ClinicDataFromDB dataFromDB = Instantiate(clinicDataFromDB, clinicDataFromDB.transform.parent);
            clinicDataFromDBStored.Add(dataFromDB);
            dataFromDB.gameObject.SetActive(true);
            dataFromDB.ClinicName.text = "Clinic Name: "+item.ClinicName;
            var doctorsInThisClinic = ReferenceManager.instance.LoginManager.signinResponse.result.doctors.Where(x => x.DoctorClinicId == item.ClinicId).ToList();
            foreach(var itemDoc in doctorsInThisClinic)
            {
                var patientsInDoctor = ReferenceManager.instance.LoginManager.signinResponse.result.patients.Where(x=>x.DoctorName == itemDoc.DoctorName).ToList();
                dataFromDB.Description.text += $"\n - <b>DR.{itemDoc.DoctorName}</b>";
                foreach (var itemPat in patientsInDoctor)
                    dataFromDB.Description.text += $"\n      -{itemPat.SubjectId}";
            }

            StartCoroutine(ReBuild(dataFromDB));
        }
    }
    public IEnumerator ReBuild(ClinicDataFromDB dataFromDB)
    {
        dataFromDB.verticalLayoutGroup.enabled = false;
        yield return new WaitForEndOfFrame();
        dataFromDB.verticalLayoutGroup.enabled = true;
    }
}
