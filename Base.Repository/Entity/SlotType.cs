using Base.Repository.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class SlotType : AuditableEntity, ICloneable
{
    [Key]
    public int SlotTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public int SessionCount { get; set; } = 3;

    public IEnumerable<Slot> Slots { get; set; } = new List<Slot>();
    public IEnumerable<Class> Classes { get; set; } = new List<Class>();

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
