using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class LogApprove
{
    public int Id { get; set; }

    public int? LeaveRequestId { get; set; }

    public int? ApprovalLevel { get; set; }

    public string? ApproverCode { get; set; }

    public string? ApproverName { get; set; }

    public string? Action { get; set; }

    public string? Comment { get; set; }

    public DateTime? ActionTime { get; set; }
}
