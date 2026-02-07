namespace AssistantCore.Companion.Messaging;

public interface ICompanionMessageHandler
{
    public void SendNotification(string topic, string title, string message);
    public void SendDataMessage(string topic, Dictionary<string, string> data);

    public void SendNotificationToDevices(string title, string message, params CompanionDevice[] devices);
    public void SendDataMessageToDevices(Dictionary<string, string> data, params CompanionDevice[] devices);
}