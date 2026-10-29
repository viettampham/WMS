using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class Department
{
    public int Id { get; set; }

    public string? DepartmentName { get; set; }

    public string? Note { get; set; }
}
