using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class DetechIssue
{
    public int Id { get; set; }

    public int IdOperationStandard { get; set; }

    public string? CodeIssue { get; set; }

    public string? DetechIssue1 { get; set; }

    public string? Type { get; set; }
}
