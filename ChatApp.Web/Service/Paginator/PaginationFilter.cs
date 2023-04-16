namespace ChatApp.Web.Service.Paginator;

public class PaginationFilter
{
    public int PageSize { get; set; }
    public string ContinuationToken { get; set; }
    
    public long LastSeenMessageTime { get; set; }
    
    public PaginationFilter()
    {
        PageSize = 50;
        ContinuationToken = "";
        LastSeenMessageTime = -1;
    }
    
    public PaginationFilter(int pageSize, string continuationToken, long lastSeenMessageTime)
    {
        PageSize = pageSize;
        ContinuationToken = continuationToken;
        LastSeenMessageTime = lastSeenMessageTime;
    }
}