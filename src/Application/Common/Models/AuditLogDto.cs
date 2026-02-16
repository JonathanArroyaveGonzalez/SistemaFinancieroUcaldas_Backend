namespace SAPFIAI.Application.Common.Models;

public class AuditLogDto
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string? UserEmail { get; set; }

    public string? Action { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; }

    public string? Details { get; set; }

    public string? Status { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ResourceId { get; set; }

    public string? ResourceType { get; set; }
}
