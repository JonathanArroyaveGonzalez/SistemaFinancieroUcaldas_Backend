namespace SAPFIAI.Domain.Entities;

public class LoginAttempt : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    
    public string IpAddress { get; set; } = string.Empty;
    
    public string? UserAgent { get; set; }
    
    public DateTime AttemptDate { get; set; }
    
    public bool WasSuccessful { get; set; }
    
    public string? FailureReason { get; set; }
    
    public LoginFailureReason? FailureReasonType { get; set; }
}
