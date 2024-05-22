using Base.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService
{
    public interface IGoogleSheetService
    {
        Task<List<Student>> GetDataFormGoogleSheets();
    }
}
