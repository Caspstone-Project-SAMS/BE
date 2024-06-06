using Base.Repository.Common;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Identity;

public class Role
{
    [Key]
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string ConcurrencyStamp { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
    public bool Deleted { get; set; } = false;

    public IEnumerable<User> Users { get; set; } = new List<User>();
}
