using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class PathConfig
{
    public int Id { get; set; }

    public string? PathName { get; set; }

    public string? PathLocation { get; set; }
}
