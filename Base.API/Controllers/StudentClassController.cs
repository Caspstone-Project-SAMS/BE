using Base.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentClassController : ControllerBase
{
    private readonly IStudentClassService _studentClassService;
    public StudentClassController(IStudentClassService studentClassService)
    {
        _studentClassService = studentClassService;
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteStudentsFromClass([FromBody] DeleteStudentClass resource)
    {
        if(ModelState.IsValid && resource.ClassId > 0)
        {
            var result = await _studentClassService.DeleteStudentsFromClass(resource.ClassId, resource.StudentIds);
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    Title = "Remove students from class successfully"
                });
            }
            return BadRequest(new
            {
                Title = "Remove students from class failed",
                Errors = result.Errors
            });
        }
        return BadRequest(new
        {
            Title = "Remove students from class failed",
            Errors = new string[1] { "Invalid input" }
        });
    }
}

public class DeleteStudentClass
{
    [Required]
    public int ClassId { get; set; }
    public IEnumerable<Guid> StudentIds { get; set; } = Enumerable.Empty<Guid>();
}
