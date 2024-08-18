using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Base.Service.ViewModel.RequestVM;

public class UserVM
{
    [Required]
    public string UserName { get; set; } = "Undefined";
    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "Undefined";
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    [EmailAddress]
    [AllowNull]
    public string? Email { get; set; }
    public int? RoleId { get; set; }
    public IFormFile? Avatar { get; set; }
    public string? FilePath { get; set; }
}

public class UpdateUserVM
{
    [EmailAddress]
    public string? Email { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    public IFormFile? Avatar { get; set; }
    public string? DisplayName { get; set; }
    public string? Address { get; set; }
    public DateTime? DOB { get; set; }
    public int? Gender { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}


