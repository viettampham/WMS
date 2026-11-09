using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class LeaveRequest
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? UserCode { get; set; }

    public string? Username { get; set; }

    public string? LeaveType { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public decimal? TotalDays { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public int? CurrentStep { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? UpdateBy { get; set; }

    public int? Stt { get; set; }

    public string? TypeShift { get; set; }

    public string? Fullname { get; set; }
}
