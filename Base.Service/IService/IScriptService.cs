using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IScriptService
{
    void ResetServerTime();
    void SetServerTime(DateTime time);
    Task AutoRegisterFingerprint();
}
