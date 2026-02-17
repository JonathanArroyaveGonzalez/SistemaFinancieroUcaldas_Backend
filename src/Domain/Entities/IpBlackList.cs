namespace SAPFIAI.Domain.Entities;

public class IpBlackList : BaseEntity
{
    public string IpAddress { get; set; } = string.Empty;
    
    public string Reason { get; set; } = string.Empty;
    
    public DateTime BlockedDate { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    public string? BlockedBy { get; set; }
    
    public bool IsActive => ExpiryDate == null || DateTime.UtcNow < ExpiryDate;
    
    public string? Notes { get; set; }
    
    public BlackListReason BlackListReason { get; set; }
}
