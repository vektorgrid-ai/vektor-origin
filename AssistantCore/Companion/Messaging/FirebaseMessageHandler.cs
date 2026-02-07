using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace AssistantCore.Companion.Messaging;

public class FirebaseMessageHandler : ICompanionMessageHandler
{
    private FirebaseMessageHandler()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefault()
            });
        }
    }

    private void SendMessage(string topic, Notification? notification = null, Dictionary<string, string>? data = null)
    {
        var message = new Message
        {
            Notification = notification,
            Data = data,
            Topic = topic
        };
        FirebaseMessaging.DefaultInstance.SendAsync(message);
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
}