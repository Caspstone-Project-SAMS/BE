using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Common;

internal class GoogleAccount
{
    public string? id { get; set; }
    public string? email { get; set; }
    public bool? verified_email { get; set; }
    public string? name { get; set; }
    public string? given_name { get; set; }
    public string? family_name { get; set; }
    public string? picture { get; set; }
}
