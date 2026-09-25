using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Avatar { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public int? FailedLoginAttempts { get; set; }

    public DateTime? LockedUntil { get; set; }

    public int RoleLevel { get; set; }

    public DateTime? CreateAt { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? EditAt { get; set; }

    public string? EditBy { get; set; }

    public DateTime? DeleteAt { get; set; }

    public string? DeleteBy { get; set; }

    public string Cccd { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhongBan { get; set; }

    public virtual Role RoleLevelNavigation { get; set; } = null!;
}
