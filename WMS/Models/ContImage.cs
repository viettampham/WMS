using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class ContImage
{
    public int Id { get; set; }

    public int? Idcont { get; set; }

    public string? Path { get; set; }

    public string? NguoiChup { get; set; }

    public DateTime? ThoiGianChup { get; set; }
}
