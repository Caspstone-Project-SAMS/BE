﻿using AutoMapper;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Vision.V1;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Base.API.Service;
using Base.Repository.Entity;
using Google.Type;
using DateTime = System.DateTime;
using System.Collections.Concurrent;
using Base.Service.Common;
using CloudinaryDotNet.Actions;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;
using Microsoft.AspNetCore.Authorization;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public ScheduleController(IScheduleService scheduleService, IMapper mapper, ICurrentUserService currentUserService)
        {
            _scheduleService = scheduleService;
            _mapper = mapper;
            _currentUserService = currentUserService;
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
        public async Task<IActionResult> CreateSchedules(List<ScheduleVM> resources,int semesterId)
        {
            try
            {
                var response = await _scheduleService.Create(resources,semesterId);
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
            [FromQuery] DateTime? endDate,
            [FromQuery] IEnumerable<int> scheduleIds)
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
                var result = await _scheduleService.GetAllSchedules(startPage, endPage, quantity, lecturerId, semesterId, startDateOnly, endDateOnly, scheduleIds);
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
                    Title = "Get schedules failed",
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Get schedules failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [Authorize(Policy = "Admin Lecturer")]
        [HttpPost("import-schedules")]
        public async Task<IActionResult> ImportSchedules([FromBody] ScheduleImport resource)
        {
            if (ModelState.IsValid)
            {
                // Check start date and end date
                if (resource.StartDate > resource.EndDate)
                {
                    return BadRequest(new
                    {
                        Title = "Import schedules failed",
                        Errors = new string[1] { "Start date must be less than or equal to end date" }
                    });
                }

                ConcurrentBag<Schedule> schedules = new ConcurrentBag<Schedule>();
                var importedDates = resource.Dates.Select(d => new DateOnly(resource.Year, d.Date.Month, d.Date.Day)).ToList();
                if(importedDates is null || importedDates.Count == 0)
                {
                    return BadRequest(new
                    {
                        Title = "Import schedules failed",
                        Errors = new string[2] { "Invalid input", "Imported date not found" }
                    });
                }

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.3 * 2))
                };
                Parallel.ForEach(resource.Slots, parallelOptions, (slot, state) =>
                {
                    Slot slotEntity = new Slot
                    {
                        SlotNumber = slot.SlotNumber
                    };
                    int indexCount = 0;
                    foreach (var importedClass in slot.AdjustedClassSlots)
                    {
                        if(importedClass is not null)
                        {
                            int dateOfWeek = (int)importedDates.ElementAt(indexCount).DayOfWeek;
                            if (dateOfWeek == 0)
                            {
                                dateOfWeek = 8;
                            }
                            else
                            {
                                dateOfWeek++;
                            }
                            var schedule = new Schedule
                            {
                                Date = importedDates.ElementAt(indexCount),
                                DateOfWeek = dateOfWeek,
                                Class = new Class
                                {
                                    ClassCode = importedClass?.ClassCode ?? string.Empty
                                },
                                Slot = slotEntity,
                                RoomID = null,
                                CreatedBy = _currentUserService.UserId,
                                CreatedAt = ServerDateTime.GetVnDateTime(),
                            };
                            schedules.Add(schedule);
                        }
                        indexCount++;
                    }
                });

                var defaultStartDate = resource.Dates.FirstOrDefault()!.Date;
                var startDate = resource.StartDate is not null ? resource.StartDate.Value : new DateOnly(resource.Year, defaultStartDate.Month, defaultStartDate.Day);
                var defaultEndDate = resource.Dates.LastOrDefault()!.Date;
                var endDate = resource.EndDate is not null ? resource.EndDate.Value : new DateOnly(resource.Year, defaultEndDate.Month, defaultEndDate.Day);

                var result = await _scheduleService.ImportSchedule(schedules.ToList(), resource.SemesterID, resource.UserID, startDate, endDate);
                if (result.IsSuccess)
                {
                    return Ok(_mapper.Map<ImportScheduleServiceResponseVM>(result));
                }

                return BadRequest(_mapper.Map<ImportScheduleServiceResponseVM>(result));
            }
            return BadRequest(new
            {
                Title = "Import schedules failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNewSchedule([FromBody] CreateScheduleVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _scheduleService.CreateNewSchedule(resource);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title,
                        Result = _mapper.Map<ScheduleResponseVM>(result.Result)
                    });
                }
                return BadRequest(new
                {
                    Title = result.Title,
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Create new schedule failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSchedule([FromBody] DeleteSchedulesVM resource)
        {
            if (ModelState.IsValid)
            {
                var result = await _scheduleService.DeleteSchedules(resource);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title
                    });
                }

                return BadRequest(new
                {
                    Title = result.Title,
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Delete schedules failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpGet("module/{id}")]
        public async Task<IActionResult> GetScheduleByIdForModule(int id)
        {
            if (ModelState.IsValid && id > 0)
            {
                var existedSchedule = await _scheduleService.GetByIdForModule(id);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduleById(int id)
        {
            if(ModelState.IsValid && id > 0)
            {
                var result = await _scheduleService.DeleteById(id);
                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        Title = result.Title
                    });
                }
                return BadRequest(new
                {
                    Title = result.Title,
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Delete schedule failed",
                Errors = new string[1] { "Invalid input" }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule([FromBody] UpdateScheduleVM resource, int id)
        {
            if(ModelState.IsValid && id > 0)
            {
                var result = await _scheduleService.UpdateSchedule(resource, id);
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
                    Title = result.Title,
                    Errors = result.Errors
                });
            }
            return BadRequest(new
            {
                Title = "Delete schedule failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }

    public class ScheduleImport
    {
        [Required]
        public Guid UserID { get; set; }
        [Required]
        public int SemesterID { get; set; }
        [Required]
        public int Year { get; set; }
        public int DatesCount { get; set; }
        public int SlotsCount { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        [Required]
        public IEnumerable<Import_Date> Dates { get; set; } = new List<Import_Date>();
        [Required]
        public IEnumerable<Import_Slot> Slots { get; set; } = new List<Import_Slot>();
    }
}
