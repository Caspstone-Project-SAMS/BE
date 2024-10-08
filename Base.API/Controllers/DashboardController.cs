﻿using Base.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("total-students")]
    public IActionResult GetTotalStudents()
    {
        return Ok(_dashboardService.GetTotalStudents());
    }

    [HttpGet("total-authenticated-students")]
    public IActionResult GetTotalAuthenticatedStudents()
    {
        return Ok(_dashboardService.GetTotalAuthenticatedStudents());
    }

    [HttpGet("total-lecturers")]
    public IActionResult GetTotalLecturers()
    {
        return Ok(_dashboardService.GetTotalLecturer());
    }

    [HttpGet("total-subjects")]
    public IActionResult GetTotalSubjects()
    {
        return Ok(_dashboardService.GetTotalSubject());
    }

    [HttpGet("total-classes")]
    public IActionResult GetTotalClasses(
        [FromQuery] int? classStatus,
        [FromQuery] int? semesterId,
        [FromQuery] int? roomId,
        [FromQuery] int? subjectId,
        [FromQuery] Guid? lecturerId)
    {
        return Ok(_dashboardService.GetTotalClass(classStatus, semesterId, roomId, subjectId, lecturerId));
    }

    [HttpGet("total-modules")]
    public IActionResult GetTotalModules()
    {
        return Ok(_dashboardService.GetTotalModules());
    }

    [HttpGet("schedules-statistic")]
    public IActionResult GetSchedulesStatistic([FromQuery] int semesterId)
    {
        return Ok(_dashboardService.GetScheduleStatistic(semesterId));
    }

    [HttpGet("module-activities-statistic")]
    public IActionResult GetModuleActivitiesStatistic([FromQuery] int semesterId)
    {
        return Ok(_dashboardService.GetModuleActivityStatistic(semesterId));
    }
}
