using System.Collections.Generic;
using UnityEngine;

public class ResearchModeHandler : MonoBehaviour
{
   public List<GameObject> ResearchItems = new List<GameObject>();

   public void ToggleResearchMode(bool value)
   {
      ReferenceManager.instance.ResearchMode = value;
      if (value)
      {
         ResearchItems.ForEach(x=>x.SetActive(true));
         ReferenceManager.instance.userReportController.userReportFromDBPrefab.CompareGaitToggle.gameObject.SetActive(true);
         ReferenceManager.instance.userReportController.userReportFromDBs.ForEach(x=>
         {
            x.CompareGaitToggle.gameObject.SetActive(true);
            x.CompareViewToggle.gameObject.SetActive(true);
         });
      }
      else
      {
         ResearchItems.ForEach(x=>x.SetActive(false));
         ReferenceManager.instance.userReportController.userReportFromDBPrefab.CompareGaitToggle.gameObject.SetActive(false);
         ReferenceManager.instance.userReportController.userReportFromDBs.ForEach(x=>
         {
            x.CompareGaitToggle.gameObject.SetActive(false);
            x.CompareViewToggle.gameObject.SetActive(false);
         });
      }
   }
}
