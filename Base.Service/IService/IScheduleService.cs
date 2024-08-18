using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService
{
    public interface IScheduleService
    {
        Task<IEnumerable<Schedule>> GetSchedules(int startPage,int endPage,Guid lecturerId,int quantity, int? semesterId,DateTime? startDate, DateTime? endDate);
        Task<ServiceResponseVM<List<ScheduleVM>>> Create(List<ScheduleVM> newEntities, int semesterId);
        Task<Schedule?> GetById(int scheduleId);
        string? GetClassCodeList(string deliminate, List<int>? scheduleIds);
        Task<ServiceResponseVM<IEnumerable<Schedule>>> GetAllSchedules(int startPage, int endPage, int quantity, Guid? lecturerId, int? semesterId, DateOnly? startDate, DateOnly? endDate);
        Task<ImportServiceResposneVM<Schedule>> ImportSchedule(List<Schedule> schedules, int semesterId, Guid userID, bool applyToSemester);
        Task<ServiceResponseVM<Schedule>> CreateNewSchedule(CreateScheduleVM resource);
        Task<ServiceResponseVM> DeleteSchedules(DeleteSchedulesVM resource);
    }
}
