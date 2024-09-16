using Expo.Server.Client;
using Expo.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Base.Service.Common;
using System.Text.Json;


namespace Base.API.Service;

public class ExpoPushNotification : IExpoPushNotification
{
    private readonly PushApiClient _expoSDKClient;
    public ExpoPushNotification()
    {
        _expoSDKClient = new PushApiClient();
    }

    public async Task Test(string expoToken, string title, string subTitle, string body)
    {
        var pushTicketReq = new PushTicketRequest()
        {
            PushTo = new List<string>() { expoToken },
            PushBadgeCount = 7,
            PushBody = body,
            PushTitle = title,
            PushSubTitle = subTitle,
            PushData = new
            {
                Event = "NewNotification",
                Data = "Go to notification"
            }
        };
        var result = await _expoSDKClient.PushSendAsync(pushTicketReq);

        if (result?.PushTicketErrors?.Count() > 0)
        {
            foreach (var error in result.PushTicketErrors)
            {
                Console.WriteLine($"Error: {error.ErrorCode} - {error.ErrorMessage}");
            }
        }

        if (result?.PushTicketStatuses?.Count() > 0) {
            foreach (var ticketStatus in result.PushTicketStatuses)
            {
                Console.WriteLine($"Status: {ticketStatus.TicketStatus}, Id: {ticketStatus.TicketId}, Message: {ticketStatus.TicketMessage}, Detail: {ticketStatus.TicketDetails.ToString()}");
            }
        }
    }

    public async Task<bool> SendMessageToMobile(string expoToken, string title, string? subtitle, string body, object? sendObject)
    {
        var jsonObject = JsonSerializer.Serialize(sendObject);
        var pushTicketReq = new PushTicketRequest()
        {
            PushTo = new List<string>() { expoToken },
            PushBody = body,
            PushTitle = title,
            PushSubTitle = subtitle,
            PushData = jsonObject
        };
        var result = await _expoSDKClient.PushSendAsync(pushTicketReq);

        if (result?.PushTicketErrors?.Count() > 0)
        {
            return false;
        }

        return true;
    }
}
