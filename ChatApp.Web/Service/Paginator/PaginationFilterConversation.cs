namespace ChatApp.Web.Service.Paginator;

public class PaginationFilterConversation
{
    public int limit { get; set; }
    public string ContinuationToken { get; set; }
    
    public long lastSeenConversationTime { get; set; }
    
    public PaginationFilterConversation()
    {
        limit = 50;
        ContinuationToken = "";
        lastSeenConversationTime = 0;
    }
    
    public PaginationFilterConversation(int limit, string continuationToken, long lastSeenMessageTime)
    {
        this.limit = limit;
        ContinuationToken = continuationToken;
        lastSeenConversationTime = lastSeenMessageTime;
    }
}