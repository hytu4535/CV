using System.ComponentModel.DataAnnotations;

namespace ShopMvc.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int? AdminUserId { get; set; }
    [Required]
    public string AdminName { get; set; } = string.Empty;
    [Required]
    public string Action { get; set; } = string.Empty;
    [Required]
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Detail { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
