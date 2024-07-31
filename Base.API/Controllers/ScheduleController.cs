using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Vision.V1;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml.Spreadsheet;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly IMapper _mapper;

        public ScheduleController(IScheduleService scheduleService, IMapper mapper)
        {
            _scheduleService = scheduleService;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ScheduleResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ScheduleResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> Get([FromQuery] int startPage, [FromQuery] int endPage, [FromQuery] Guid lecturerId, [FromQuery] int quantity, [FromQuery] int? semesterId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            if (ModelState.IsValid)
            {
                var schedule = await _scheduleService.GetSchedules(startPage, endPage, lecturerId, quantity, semesterId, startDate, endDate);
                var result = _mapper.Map<IEnumerable<ScheduleResponse>>(schedule);
                if (result.Count() <= 0)
                {
                    return NotFound("Lecturer not have any Schedule");
                }
                return Ok(result);
            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> CreateSchedules(List<ScheduleVM> resources)
        {
            try
            {
                var response = await _scheduleService.Create(resources);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            if (ModelState.IsValid && id > 0)
            {
                var existedSchedule = await _scheduleService.GetById(id);
                if (existedSchedule is null)
                {
                    return NotFound(new
                    {
                        Title = "Schedule not found"
                    });
                }
                return Ok(new
                {
                    Result = _mapper.Map<ScheduleResponseVM>(existedSchedule)
                });
            }
            return BadRequest(new
            {
                Title = "Get schedule information failed",
                Errors = new string[1] { "Invalid input" }
            });
        }


        [HttpGet("test-get-all")]
        public async Task<IActionResult> GetALlSchedules(
            [FromQuery] int startPage,
            [FromQuery] int endPage,
            [FromQuery] int quantity,
            [FromQuery] Guid? lecturerId,
            [FromQuery] int? semesterId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            if (ModelState.IsValid)
            {
                DateOnly? startDateOnly = null;
                DateOnly? endDateOnly = null;
                if (startDate is not null)
                {
                    startDateOnly = DateOnly.FromDateTime(startDate.Value);
                }
                if (endDate is not null)
                {
                    endDateOnly = DateOnly.FromDateTime(endDate.Value);
                }
                var result = await _scheduleService.GetAllSchedules(startPage, endPage, quantity, lecturerId, semesterId, startDateOnly, endDateOnly);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = result.Result
                    });
                }
                return BadRequest(new
                {
                    Title = "Get schedules falied",
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Get schedules failed",
                Errors = new string[1] { "Invalid input" }
            });
        }


        [HttpPost("import-v1/block/with-space")]
        public async Task<IActionResult> ImportSchedulesV1([FromForm] ImportSchedule resource)
        {
            var credential = GoogleCredential.FromFile("keys/next-project-426205-5bd6e4b638be.json");
            ImageAnnotatorClientBuilder imageAnnotatorClientBuilder = new ImageAnnotatorClientBuilder();
            imageAnnotatorClientBuilder.Credential = credential;
            var client = imageAnnotatorClientBuilder.Build();
            Image image = Image.FromStream(resource.Image!.OpenReadStream());

            // Perform text detection on the image
            var response = await client.DetectDocumentTextAsync(image);
            var texts = new List<string>();

            foreach (var page in response.Pages)
            {
                foreach (var block in page.Blocks)
                {
                    string blockText = "";
                    foreach (var paragraph in block.Paragraphs)
                    {
                        foreach (var word in paragraph.Words)
                        {
                            foreach (var symbol in word.Symbols)
                            {
                                blockText += symbol.Text;
                            }
                            blockText += " "; // Add space after each word
                        }
                    }

                    texts.Add(blockText);


                    // Get the bounding box for the block
                    var boundingBox = block.BoundingBox;
                    Console.WriteLine($"Detected block text: {blockText.Trim()}");
                    Console.WriteLine("Bounding Box Vertices:");
                    foreach (var vertex in boundingBox.Vertices)
                    {
                        Console.WriteLine($"({vertex.X}, {vertex.Y})");
                    }
                }
            }

            return Ok(texts);
        }


        [HttpPost("import-v2/block/no-space")]
        public async Task<IActionResult> ImportSchedulesV2([FromForm] ImportSchedule resource)
        {
            var credential = GoogleCredential.FromFile("keys/next-project-426205-5bd6e4b638be.json");
            ImageAnnotatorClientBuilder imageAnnotatorClientBuilder = new ImageAnnotatorClientBuilder();
            imageAnnotatorClientBuilder.Credential = credential;
            var client = imageAnnotatorClientBuilder.Build();
            Image image = Image.FromStream(resource.Image!.OpenReadStream());

            // Perform text detection on the image
            var response = await client.DetectDocumentTextAsync(image);
            var texts = new List<string>();

            foreach (var page in response.Pages)
            {
                foreach (var block in page.Blocks)
                {
                    string blockText = "";
                    foreach (var paragraph in block.Paragraphs)
                    {
                        foreach (var word in paragraph.Words)
                        {
                            foreach (var symbol in word.Symbols)
                            {
                                blockText += symbol.Text;
                            }
                        }
                    }

                    texts.Add(blockText);

                    // Get the bounding box for the block
                    var boundingBox = block.BoundingBox;
                    Console.WriteLine($"Detected block text: {blockText.Trim()}");
                    Console.WriteLine("Bounding Box Vertices:");
                    foreach (var vertex in boundingBox.Vertices)
                    {
                        Console.WriteLine($"({vertex.X}, {vertex.Y})");
                    }
                }
            }

            return Ok(texts);
        }


        [HttpPost("import-v3/word")]
        public async Task<IActionResult> ImportSchedulesV3([FromForm] ImportSchedule resource)
        {
            var credential = GoogleCredential.FromFile("keys/next-project-426205-5bd6e4b638be.json");
            ImageAnnotatorClientBuilder imageAnnotatorClientBuilder = new ImageAnnotatorClientBuilder();
            imageAnnotatorClientBuilder.Credential = credential;
            var client = imageAnnotatorClientBuilder.Build();
            Image image = Image.FromStream(resource.Image!.OpenReadStream());

            // Perform text detection on the image
            var response = await client.DetectDocumentTextAsync(image);
            var texts = new List<string>();

            foreach (var page in response.Pages)
            {
                foreach (var block in page.Blocks)
                {
                    foreach (var paragraph in block.Paragraphs)
                    {
                        foreach (var word in paragraph.Words)
                        {
                            string blockText = "";
                            foreach (var symbol in word.Symbols)
                            {
                                blockText += symbol.Text;
                            }
                            texts.Add(blockText);
                        }
                    }

                }
            }

            return Ok(texts);
        }
    }
}
