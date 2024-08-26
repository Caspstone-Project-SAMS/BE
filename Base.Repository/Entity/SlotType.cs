using Base.Repository.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class SlotType : AuditableEntity
{
    [Key]
    public int SlotTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public int SessionCount { get; set; } = 3;

    public IEnumerable<Slot> Slots { get; set; } = Enumerable.Empty<Slot>();
    public IEnumerable<Class> Classes { get; set; } = Enumerable.Empty<Class>();
}
