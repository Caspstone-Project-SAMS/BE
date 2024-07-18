using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadTemplateController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public DownloadTemplateController(IWebHostEnvironment hostingEnvironment)
        {

            _hostingEnvironment = hostingEnvironment;

        }


        [HttpGet("download-excel-template-class")]
        public IActionResult DownloadExcelForClass()
        {
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "template_class.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "template_class.xlsx");
        }


        [HttpGet("download-excel-template-student")]
        public IActionResult DownloadExcelForStudent()
        {
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "template_student.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "template_student.xlsx");
        }
        [HttpGet("download-excel-template-schedule")]
        public IActionResult DownloadExcelSchedule()
        {
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "template_schedule.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "template_schedule.xlsx");
        }

    }
}
