namespace SAPFIAI.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;
    
    public DateTime ExpiryDate { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public bool IsRevoked { get; set; }
    
    public DateTime? RevokedDate { get; set; }
    
    public string? ReplacedByToken { get; set; }
    
    public string? CreatedByIp { get; set; }
    
    public string? RevokedByIp { get; set; }
    
    public string? ReasonRevoked { get; set; }

    // Computed property
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiryDate;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
}
