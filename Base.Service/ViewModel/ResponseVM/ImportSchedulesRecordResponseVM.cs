using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class ImportSchedulesRecordResponseVM
{
    public int ImportSchedulesRecordID { get; set; }
    public string? Title { get; set; }
    public DateTime? RecordTimestamp { get; set; }
    public bool? ImportReverted { get; set; }
    public bool? IsReversible { get; set; }
    public User_ImportSchedulesRecordResponseVM? User { get; set; }

}

public class User_ImportSchedulesRecordResponseVM
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
}
