using Base.Repository.Common;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Identity;

public class Role : IdentityRole<Guid>
{
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
    public IEnumerable<User> Users { get; set; } = new List<User>();

    public bool Deleted { get; set; } = false;
}
