using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace AssistantCore.Companion.Messaging;

public class FirebaseMessageHandler : ICompanionMessageHandler
{
    public FirebaseMessageHandler()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                // This will use the GOOGLE_APPLICATION_CREDENTIALS environment variable
                Credential = GoogleCredential.GetApplicationDefault()
            });
        }
    }

    private static void SendMessage(string topic, Notification? notification = null, Dictionary<string, string>? data = null)
    {
        var message = new Message
        {
            Notification = notification,
            Data = data,
            Topic = topic
        };
        FirebaseMessaging.DefaultInstance.SendAsync(message);
    }
    private static void SendMessageToDevices(CompanionDevice[] devices, 
        Notification? notification = null, 
        Dictionary<string, string>? data = null)
    {
        var message = new MulticastMessage
        {
            Notification = notification,
            Data = data,
            Tokens = devices.Select(d => d.FirebaseToken).Where(t => t != null).ToList()
        };
        FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
    }

    public void SendNotification(string topic, string title, string message)
    {
        var notification = new Notification
        {
            Title = title,
            Body = message
        };
        SendMessage(topic, notification: notification);
    }

    public void SendDataMessage(string topic, Dictionary<string, string> data)
    {
        SendMessage(topic, data: data);
    }

    public void SendNotificationToDevices(string title, string message, params CompanionDevice[] devices)
    {
        var notification = new Notification
        {
            Title = title,
            Body = message
        };
        SendMessageToDevices(devices, notification: notification);
    }

    public void SendDataMessageToDevices(Dictionary<string, string> data, params CompanionDevice[] devices)
    {
        SendMessageToDevices(devices, data: data);
    }
}