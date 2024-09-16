using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Common;

public interface IWebsocketNotificationService
{
    Task<bool> SendMessageToClient(string message, Guid userId);
}
