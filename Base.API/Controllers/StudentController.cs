using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        public class Student
        {
            public string? StudentCode { get; set; }
            public string? FullName { get; set; }
        }
        [HttpPost("upload")]
        public IActionResult UploadExelFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);

                        List<Student> students = new List<Student>();
                        foreach (var row in worksheet.RowsUsed().Skip(1))
                        {
                            var code = row.Cell(1).Value.ToString();
                            var name = row.Cell(2).Value.ToString();
                            students.Add(new Student { StudentCode = code, FullName = name });
                        }
                        return Ok(students);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
