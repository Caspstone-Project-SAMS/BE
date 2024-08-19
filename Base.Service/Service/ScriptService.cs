using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class ScriptService : IScriptService
{
    private IUnitOfWork _unitOfWork;

    public ScriptService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void SetServerTime(DateTime time)
    {
        // Setup server datetime
        ServerDateTime.SetServerTime(time);

        // setup job at the past
        var api = JobStorage.Current.GetMonitoringApi();
        //var pastJob = api.
    }

    public async Task AutoRegisterFingerprint()
    {
        var fingerprints = _unitOfWork.StoredFingerprintDemoRepository
            .Get(s => s.FingerprintTemplate != string.Empty)
            .AsNoTracking()
            .ToArray();

        var unauthenticatedStudents = _unitOfWork.StudentRepository
            .Get(s => !s.IsDeleted && s.FingerprintTemplates.Count() == 0, 
            new System.Linq.Expressions.Expression<Func<Repository.Entity.Student, object?>>[]
            {
                s => s.FingerprintTemplates
            })
            .AsNoTracking()
            .ToList();

        var addedFingerprints = new List<FingerprintTemplate>();
        int fingerprintIndex = 0;
        var fingersTotal = fingerprints.Count() - 1;

        foreach (var student in unauthenticatedStudents)
        {
            addedFingerprints.Add(new FingerprintTemplate
            {
                FingerprintTemplateData = fingerprints[fingerprintIndex++].FingerprintTemplate,
                Status = 1,
                StudentID = student.StudentID,
            });
            if (fingerprintIndex > fingersTotal)
            {
                fingerprintIndex = 0;
            }

            addedFingerprints.Add(new FingerprintTemplate
            {
                FingerprintTemplateData = fingerprints[fingerprintIndex++].FingerprintTemplate,
                Status = 1,
                StudentID = student.StudentID,
            });
            if(fingerprintIndex > fingersTotal)
            {
                fingerprintIndex = 0;
            }
        }

        await _unitOfWork.FingerprintRepository.AddRangeAsync(addedFingerprints);
        await _unitOfWork.SaveChangesAsync();
    }
}
