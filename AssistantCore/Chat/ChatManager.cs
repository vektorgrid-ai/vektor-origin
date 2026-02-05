using System.Collections.Concurrent;

namespace AssistantCore.Chat;

public class ChatManager
{
    private Guid _chatId;
    private ConcurrentQueue<ChatEvent> _chatEvents;
    private TimeSpan _expirationDuration;
    private DateTime _lastChat;

    private ChatManager(List<ChatEvent> chatEvents)
    {
        _chatEvents = new ConcurrentQueue<ChatEvent>(chatEvents);
    }

    public static ChatManager Create(TimeSpan expirationDuration)
    {
        var mgr = new ChatManager([])
        {
            _expirationDuration = expirationDuration,
            _chatId = Guid.NewGuid(),
            _lastChat = DateTime.UtcNow
        };

        return mgr;
    }
    
    public void AddEvent(ChatEvent chatEvent)
    {
        EnsureNotExpired();
        _chatEvents.Enqueue(chatEvent);
        _lastChat = DateTime.UtcNow;
    }
    
    public bool IsExpired()
    {
        return DateTime.UtcNow - _lastChat > _expirationDuration;
    }

    private void EnsureNotExpired()
    {
        if (!IsExpired()) return;
        
        _chatEvents = [];
        _chatId = Guid.NewGuid();
        _lastChat = DateTime.UtcNow;
    }
    
    public ChatContext GetContext()
    {
        EnsureNotExpired();
        return new ChatContext
        {
            ChatId = _chatId,
            Events = new List<ChatEvent>(_chatEvents)
        };
     
    }
}