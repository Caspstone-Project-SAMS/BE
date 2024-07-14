﻿using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IEmployeeService
{
    Task<User?> GetById(Guid id);
}
