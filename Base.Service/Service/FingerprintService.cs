using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class FingerprintService : IFingerprintService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;

    public FingerprintService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<ServiceResponseVM<FingerprintTemplate>> CreateNewFinger(Guid studentId, string fingerprintTemplate)
    {
        var includes = new Expression<Func<Student, object?>>[]
        {
            s => s.FingerprintTemplates
        };
        var result = new ServiceResponseVM<FingerprintTemplate>()
        {
            IsSuccess = false
        };
        var existedStudent = _unitOfWork.StudentRepository
            .Get(s => !s.IsDeleted && s.StudentID == studentId, includes)
            .FirstOrDefault();
        if(existedStudent is null)
        {
            result.Title = "Register fingerprint failed";
            result.Errors = new string[1] { "Student not found" };
            return result;
        }

        if (existedStudent.FingerprintTemplates.Count() >= 2)
        {
            result.Title = "Register fingerprint failed";
            result.Errors = new string[1] { "Student already register 2 fingers" };
            return result;
        }

        var fingers = existedStudent.FingerprintTemplates.ToList();
        var newFinger = new FingerprintTemplate
        {
            Status = 1,
            FingerprintTemplateData = fingerprintTemplate,
            CreatedAt = DateTime.Now,
        };
        fingers.Add(newFinger);
        existedStudent.FingerprintTemplates = fingers;

        try
        {
            var saveResult = await _unitOfWork.SaveChangesAsync();

            if (saveResult)
            {
                result.IsSuccess = true;
                result.Title = "Register fingerprint successfully";
                result.Result = newFinger;
                return result;
            }
            else
            {
                result.Title = "Register fingerprint failed";
                result.Errors = new string[1] { "Save changes failed" };
                return result;
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponseVM<FingerprintTemplate>
            {
                IsSuccess = false,
                Title = "Register fingerprint failed",
                Errors = new string[1] { ex.Message }
            };
        }
        catch (OperationCanceledException ex)
        {
            return new ServiceResponseVM<FingerprintTemplate>
            {
                IsSuccess = false,
                Title = "Register fingerprint failed",
                Errors = new string[2] { "The operation has been cancelled", ex.Message }
            };
        }
    }
}
