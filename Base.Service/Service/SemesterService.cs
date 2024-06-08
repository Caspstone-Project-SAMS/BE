using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SemesterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Semester>> GetSemester()
        {
            return await _unitOfWork.SemesterRepository.FindAll().ToListAsync();
        }
    }
}
