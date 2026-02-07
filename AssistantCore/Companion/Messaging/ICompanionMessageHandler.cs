namespace AssistantCore.Companion.Messaging;

public interface ICompanionMessageHandler
{
    public void SendNotification(string topic, string title, string message);
    public void SendDataMessage(string topic, Dictionary<string, string> data);
}