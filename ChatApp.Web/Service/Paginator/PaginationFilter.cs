namespace ChatApp.Web.Service.Paginator;

public class PaginationFilter
{
    public int limit { get; set; }
    public string ContinuationToken { get; set; }
    
    public long lastSeenMessageTime { get; set; }
    
    public PaginationFilter()
    {
        limit = 50;
        ContinuationToken = "";
        lastSeenMessageTime = 0;
    }
    
    public PaginationFilter(int limit, long lastSeenMessageTime, string continuationToken)
    {
        this.limit = limit;
        ContinuationToken = continuationToken;
        this.lastSeenMessageTime = lastSeenMessageTime;
    }
}