namespace ChatApp.Web.Service.Paginator;

public class PaginationFilter
{
    public int PageSize { get; set; }
    public string ContinuationToken { get; set; }
    
    public long LastSeenMessageTime { get; set; }
    
    public PaginationFilter()
    {
        this.PageSize = 50;
        this.ContinuationToken = "";
        this.LastSeenMessageTime = -1;
    }
    
    public PaginationFilter(int pageSize, string continuationToken, long lastSeenMessageTime)
    {
        this.PageSize = pageSize;
        this.ContinuationToken = continuationToken;
        this.LastSeenMessageTime = lastSeenMessageTime;
    }
}