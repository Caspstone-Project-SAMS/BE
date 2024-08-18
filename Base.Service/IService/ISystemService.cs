using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface ISystemService
{
    Task<SystemConfiguration> GetSystemConfiguration();
    Task<ServiceResponseVM<SystemConfiguration>> UpdateConfiguration(SystemConfigurationVM resource);
}
