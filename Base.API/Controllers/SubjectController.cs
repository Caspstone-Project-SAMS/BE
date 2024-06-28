using AutoMapper;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Service;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Base.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;
        public SubjectController(ISubjectService subjectService, IMapper mapper)
        {
            _subjectService = subjectService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubject()
        {
            var subjects = await _subjectService.Get();
            return Ok(_mapper.Map<IEnumerable<SubjectResponse>>(subjects));      
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetSubjectByID([FromQuery]int id)
        {
            var subject = await _subjectService.GetById(id);
            if (subject == null)
            {
                return NotFound($"Subject with ID ={id} not found");
            }
            return Ok(_mapper.Map<SubjectResponse>(subject));
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewSubject(SubjectVM resource)
        {
            var result = await _subjectService.Create(resource);
            if(result.IsSuccess)
            {
                return Ok("Create new subject successfully");
            }
            return BadRequest(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSubject(SubjectVM resource, int id)
        {
            var result = await _subjectService.Update(resource, id);
            if (result.IsSuccess)
            {
                return Ok("Update subject successfully");
            }
            return BadRequest(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            if (ModelState.IsValid)
            {
                var result = await _subjectService.Delete(id);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest(new
            {
                Title = "Delete subject failed",
                Errors = new string[1] { "Invalid input" }
            });
        }
    }
}
