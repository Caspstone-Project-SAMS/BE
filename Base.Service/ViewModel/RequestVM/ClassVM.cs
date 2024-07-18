using Base.Repository.Entity;
using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class ClassVM
{
    [Required]
    public string ClassCode { get; set; } = string.Empty;
    [Required]
    public int SemesterId { get; set; }
    [Required]
    public int RoomId { get; set; }
    [Required]
    public int SubjectId { get; set; }
    [Required]
    public Guid LecturerID { get; set; }

}