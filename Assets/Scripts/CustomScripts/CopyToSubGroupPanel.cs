using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class CopyToSubGroupPanel : MonoBehaviour
{
    public long reportId;

    public TMP_InputField GroupName;
    public TMP_InputField SubGroupName;

    public void SetReportRecordId(UserReportFromDB value)
    {
        reportId = value.videoId;
    }
    public void PerformCopyToAction()
    {
        if (string.IsNullOrEmpty(GroupName.text) || string.IsNullOrEmpty(SubGroupName.text) || ReferenceManager.instance.userReportController.addedReportGroupHandlers.FirstOrDefault(x =>
                x.GroupName.text == GroupName.text) == null)
        {
            ReferenceManager.instance.PopupManager.Show("Incorrect group name!","Please enter a Group/Subgroup name correctly");
            return;
        }
        CopyToGroupRequest copyToGroupRequest = new CopyToGroupRequest()
        {
            ReportId = reportId,
            GroupName = GroupName.text,
            SubgroupName = SubGroupName.text
        };
        string json = JsonConvert.SerializeObject(copyToGroupRequest);
        APIHandler.instance.Post("UserReport/CopyReportToOtherGroup",json,onSuccess:(response)=>
        {
            ResponseWithNoObject responseWithNoObject = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
            if (responseWithNoObject.isSuccess)
            {
                ReferenceManager.instance.PopupManager.Show("Success",$"Capture Coppied to {copyToGroupRequest.GroupName}/{copyToGroupRequest.SubgroupName} successfully");
            }
            else if (responseWithNoObject.isError)
            {
                string reasons = "";
                foreach (var item in responseWithNoObject.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }

                ReferenceManager.instance.PopupManager.Show(
                    "Copy To Group Failed",
                    $"Reasons are: {reasons}"
                );
            }
        },onError: (error) =>
        {
            ReferenceManager.instance.PopupManager.Show(
                "Copy To Group Failed",
                $"Reasons are: {error}"
            );
        });
    }
}
