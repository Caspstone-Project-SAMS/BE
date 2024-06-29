using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateGet _validateGet;
        public ScheduleService(IUnitOfWork unitOfWork, IValidateGet validateGet)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
        }
        public async Task<IEnumerable<Schedule>> GetSchedules(int startPage, int endPage, Guid lecturerId, int quantity, int semesterId, DateTime? startDate, DateTime? endDate)
        {
            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                throw new ArgumentException("Error when get quantity per page");
            }

            var query = await _unitOfWork.ScheduleRepository.Get(s => s.Class!.LecturerID == lecturerId && s.Class.SemesterID == semesterId)
                .Include(s => s.Class)
                .Include(s => s.Class!.Semester)
                .Include(s => s.Class!.Room)
                .Include(s => s.Slot)
                .Include(s => s.Class!.Subject).ToArrayAsync();

            if (startDate.HasValue)
            {
                var startDateOnly = DateOnly.FromDateTime(startDate.Value);
                query = query.Where(s => s.Date >= startDateOnly).ToArray();

            }
            if (endDate.HasValue)
            {
                var endDateOnly = DateOnly.FromDateTime(endDate.Value);
                query = query.Where(s => s.Date <= endDateOnly).ToArray();
            }

            var schedules = query
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult);

            return schedules;
        }
    }
}
