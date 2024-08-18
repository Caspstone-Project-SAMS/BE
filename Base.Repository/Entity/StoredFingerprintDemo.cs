using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class StoredFingerprintDemo
{
    [Key]
    public int Id { get; set; }
    public string FingerprintTemplate { get; set; } = string.Empty;
}
