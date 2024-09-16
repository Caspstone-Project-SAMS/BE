using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Common;

public interface IExpoPushNotification
{
    Task Test(string expoToken, string title, string subTitle, string body);
    Task<bool> SendMessageToMobile(string expoToken, string title, string? subtitle, string body, object? sendObject);
}