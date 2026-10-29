using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class CtpatTopic
{
    public int Id { get; set; }

    public string? Topic { get; set; }

    public string? Note { get; set; }

    public string? TakeOn { get; set; }
}
