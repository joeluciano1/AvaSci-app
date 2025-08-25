using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CustomDropDown : MonoBehaviour
{
   public CustomDropDownItem itemPrefab;
   public List<UserReportFromDB> selectedUserReports = new List<UserReportFromDB>();
   public bool generated;
   public List<CustomDropDownItem> items = new List<CustomDropDownItem>();
   public List<CustomDropDownItem> selectedItems = new List<CustomDropDownItem>();
   private void Awake()
   {
      if (generated)
      {
         return;
      }
      foreach (var userReportFromDB in ReferenceManager.instance.userReportController.userReportFromDBs)
      {
         if (userReportFromDB.timeBasedReadings!=null && userReportFromDB.timeBasedReadings.Count>0  && userReportFromDB.gaitReports!=null && userReportFromDB.gaitReports.Count >0)
         {
            if(GeneralStaticManager.AreAllNullablePropertiesNull(userReportFromDB.timeBasedReadings[3]) && GeneralStaticManager.AreAllNullablePropertiesNull(userReportFromDB.gaitReports[3])){
               continue;
            }
         }
         var item = Instantiate(itemPrefab, itemPrefab.transform.parent);
         item.gameObject.SetActive(true);
         item.text.text = $"{userReportFromDB.UserNamefromDB.text}\n{userReportFromDB.ReportDescription.text}";
         item.myUserReport = userReportFromDB;
         if (!items.Contains(item))
         {
            items.Add(item);
         }
      }

      generated = true;
   }

   public void Search(string searchText)
   {
      Debug.Log(searchText);
      if (string.IsNullOrEmpty(searchText))
      {
         items.ForEach(x=>x.gameObject.SetActive(true));
         return;
      }
      items.ForEach(x=>x.gameObject.SetActive(false));
      items.Where(x=>x.text.text.ToLower().Contains(searchText.ToLower())).ToList().ForEach(x=>x.gameObject.SetActive(true));
   }
}
