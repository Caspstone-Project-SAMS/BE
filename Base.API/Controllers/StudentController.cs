using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Google.Apis.Sheets.v4;
using Base.API.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;
using Base.Repository.Entity;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System;
using Base.Service.ViewModel.RequestVM;
using Base.IService.IService;
using AutoMapper;
using Base.Service.ViewModel.ResponseVM;
using Base.Service.Service;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        const string SPREADSHEET_ID = "1euZpP6axqFteYbSY6qFhiWoYCobtMQ4qX1QO_OL9u2w";
        const string SHEET_NAME = "Student";
        SpreadsheetsResource.ValuesResource _googleSheetValues;
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;

        public StudentController(GoogleSheetsHelper googleSheetsHelper, IStudentService studentService, IMapper mapper)
        {
            _googleSheetValues = googleSheetsHelper.Service.Spreadsheets.Values;
            _studentService = studentService;
            _mapper = mapper;
        }
        //public class Student
        //{
        //    public string? StudentCode { get; set; }
        //    public string? FullName { get; set; }
        //    public string? Status { get; set; }
        //}
        //[HttpGet]
        //public IActionResult Get(string studentCode)
        //{
        //    try
        //    {
        //        var range = $"{SHEET_NAME}!A:D";
        //        var request = _googleSheetValues.Get(SPREADSHEET_ID, range);
        //        var response = request.Execute();
        //        var values = response.Values;
        //        var students = new List<Student>();
        //        foreach (var value in values)
        //        {
        //            Student student = new()
        //            {
        //                StudentCode = value[0].ToString(),
        //                FullName = value[1].ToString()
        //            };
        //            students.Add(student);
        //        }
        //        var test = students.Where(s => s.StudentCode == studentCode.ToUpper()).SingleOrDefault();
        //        return Ok(test);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        //[HttpPost("upload-gg-sheet")]
        //public IActionResult Post(IFormFile file)
        //{
        //    var range = $"{SHEET_NAME}!A:D";
        //    var valueRange = new ValueRange();
        //    List<Student> students = new List<Student>();
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    using (var stream = new MemoryStream())
        //    {
        //        file.CopyTo(stream);
        //        using (var workbook = new XLWorkbook(stream))
        //        {
        //            var worksheet = workbook.Worksheet(1);
        //            foreach (var row in worksheet.RowsUsed().Skip(1))
        //            {
        //                var code = row.Cell(1).Value.ToString();
        //                var name = row.Cell(2).Value.ToString();
        //                var status = row.Cell(3).Value.ToString();
        //                students.Add(new Student { StudentCode = code, FullName = name , Status = status});
        //            }
        //        }
        //    }
        //    var rangeData = new List<IList<object>>();
        //    foreach (var student in students)
        //    {
        //        var objectList = new List<object> { student.StudentCode!, student.FullName!,student.Status! };
        //        rangeData.Add(objectList);
        //    }

        //    valueRange.Values = rangeData;

        //    var appendRequest = _googleSheetValues.Append(valueRange, SPREADSHEET_ID, range);
        //    appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        //    appendRequest.Execute();

        //    return CreatedAtAction(nameof(Get),$"Have {students.Count} new student was added");
        //}
        //[HttpPut("update-status-gg-sheet")]
        //public IActionResult UpdateStatus( [FromBody] string status,string studentCode)
        //{
        //    var start = DateTime.Now;
        //    int rowId = 0;
        //    var columnRange =$"{SHEET_NAME}!A:A"; ;
        //    var request = _googleSheetValues.Get(SPREADSHEET_ID, columnRange);
        //    var response = request.Execute();
        //    var values = response.Values;
        //    var end = DateTime.Now;

        //    if (values != null)
        //    {
        //        for (int i = 0; i < values.Count; i++)
        //        {
        //            if (values[i].Count > 0 && values[i][0].ToString() == studentCode)
        //            {
        //                rowId += i + 1;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        rowId = -1;
        //    }
           
        //    if (rowId > 0)
        //    {
        //        var range = $"{SHEET_NAME}!C{rowId}";
        //        var objectList = new List<object>() { status };
        //        var rangeData = new List<IList<object>> { objectList };
        //        var valueRange = new ValueRange
        //        {
        //            Values = rangeData
        //        };
        //        var updateRequest = _googleSheetValues.Update(valueRange, SPREADSHEET_ID, range);
        //        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.RAW;
        //        updateRequest.Execute();
        //        var time = end - start;
        //        return Ok(time.TotalSeconds);
        //    }
        //    else
        //    {
        //        return NotFound("StudentCode not existed");
        //    }
        //}
        //[HttpPost("upload")]
        //public IActionResult UploadExelFile(IFormFile file)
        //{
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //        {
        //            return BadRequest("No file uploaded.");
        //        }

        //        using (var stream = new MemoryStream())
        //        {
        //            file.CopyTo(stream);
        //            using (var workbook = new XLWorkbook(stream))
        //            {
        //                var worksheet = workbook.Worksheet(1);

        //                List<Student> students = new List<Student>();
        //                foreach (var row in worksheet.RowsUsed().Skip(1))
        //                {
        //                    var code = row.Cell(1).Value.ToString();
        //                    var name = row.Cell(2).Value.ToString();
        //                    students.Add(new Student { StudentCode = code, FullName = name });
        //                }
        //                return Ok(students);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        //[HttpGet("export-gg-sheet")]
        //public IActionResult ExportExcel()
        //{
        //    try
        //    {
        //        List<Student> students = new List<Student>
        //        {
        //            new Student { FullName = "Trần Quốc", StudentCode = "xExx1503" },
        //            new Student { FullName = "Lê Khoa", StudentCode = "xE1xx4x4" },
        //            new Student { FullName = "Nguyễn Đức", StudentCode = "xExx6xx58" }
        //        };
        //        using (var workbook = new XLWorkbook())
        //        {
        //            var worksheet = workbook.Worksheets.Add("Students");
        //            worksheet.Cell(1, 1).Value = "Student Name";
        //            worksheet.Cell(1, 2).Value = "Student ID";

        //            for (int i = 0; i < students.Count; i++)
        //            {
        //                worksheet.Cell(i + 2, 1).Value = students[i].FullName;
        //                worksheet.Cell(i + 2, 2).Value = students[i].StudentCode;
        //            }

        //            using (var stream = new MemoryStream())
        //            {
        //                workbook.SaveAs(stream);
        //                var content = stream.ToArray();
        //                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "students.xlsx");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> GetAllStudents([FromQuery]int startPage, [FromQuery] int endPage, [FromQuery] int? quantity, [FromQuery] Guid? studentID, [FromQuery] string? studentCode, [FromQuery] bool isModule = false)
        {
            if (ModelState.IsValid)
            {
                var students = await _studentService.GetStudents(startPage, endPage, quantity, studentID,studentCode);
                if(isModule)
                {
                    return Ok(_mapper.Map<IEnumerable<StudentModuleResponse>>(students));
                }
                return Ok(_mapper.Map<IEnumerable<StudentResponse>>(students));
            }
            
            return BadRequest(new
            {
                Title = "Get Students information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewStudent(List<StudentVM> resources)
        {    
                var result = await _studentService.CreateStudent(resources);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
        }

        [HttpGet("get-students-by-classId")]
        public async Task<IActionResult> GetAllStudents([FromQuery] int classID, [FromQuery] int startPage, [FromQuery] int endPage, [FromQuery] int? quantity, [FromQuery] bool isModule = false)
        {
            if (ModelState.IsValid)
            {
                var students = await _studentService.GetStudentsByClassID(classID,startPage,endPage,quantity);
                if (isModule)
                {
                    return Ok(_mapper.Map<IEnumerable<StudentModuleResponse>>(students));
                }
                return Ok(_mapper.Map<IEnumerable<StudentResponse>>(students));
            }

            return BadRequest(new
            {
                Title = "Get Students information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            if (ModelState.IsValid)
            {
                var result = await _studentService.Delete(id);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest(new
            {
                Title = "Delete student failed",
                Errors = new string[1] { "Invalid input" }
            });
        }


    }
}
