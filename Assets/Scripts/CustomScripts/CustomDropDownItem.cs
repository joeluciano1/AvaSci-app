using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomDropDownItem : MonoBehaviour
{
    public TMP_Text text;
    public UserReportFromDB myUserReport;
    public CustomDropDown myDropDown;
    public Toggle myToggle;
    public void ToggleUserReport(bool value)
    {
        if (value)
        {
            myDropDown.selectedUserReports.Add(myUserReport);
            myDropDown.selectedItems.Add(this);
        }
        else
        {
            myDropDown.selectedUserReports.Remove(myUserReport);
            myDropDown.selectedItems.Remove(this);
        }
    }
}
