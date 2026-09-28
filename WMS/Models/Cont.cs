using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class Cont
{
    public int Id { get; set; }

    public string? SoCont { get; set; }

    public int? SoLuongAnh { get; set; }

    public string? NguoiKhaiBao { get; set; }

    public string? SoBooking { get; set; }
}
