using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class OperationStandard
{
    public int Id { get; set; }

    public string OperationStandard1 { get; set; } = null!;

    public string? Note { get; set; }
}
