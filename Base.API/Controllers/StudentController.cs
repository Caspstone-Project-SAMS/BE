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
        [HttpGet("export")]
        public IActionResult ExportExcel()
        {
            try
            {
                List<Student> students = new List<Student>
                {
                    new Student { FullName = "Trần Quốc", StudentCode = "xExx1503" },
                    new Student { FullName = "Lê Khoa", StudentCode = "xE1xx4x4" },
                    new Student { FullName = "Nguyễn Đức", StudentCode = "xExx6xx58" }
                };
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Students");
                    worksheet.Cell(1, 1).Value = "Student Name";
                    worksheet.Cell(1, 2).Value = "Student ID";

                    for (int i = 0; i < students.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = students[i].FullName;
                        worksheet.Cell(i + 2, 2).Value = students[i].StudentCode;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "students.xlsx");
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
