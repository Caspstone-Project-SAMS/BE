using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Common
{
    public interface ICurrentUserService
    {
        public string UserId { get; }

        public string UserName { get; }

        IEnumerable<string> Roles { get; }
    }
}
