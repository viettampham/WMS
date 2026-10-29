using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class CtpatCriterion
{
    public int Id { get; set; }

    public int? Idtopic { get; set; }

    public string? Criteria { get; set; }

    public string? Note { get; set; }
}
