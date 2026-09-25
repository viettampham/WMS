using System;
using System.Collections.Generic;

namespace WMS.Models;

public partial class Role
{
    public int Id { get; set; }

    public int RoleLevel { get; set; }

    public string? RoleName { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
