namespace ActPro.Domain.Models.Areas
{
    public class AuditLogsViewModel
    {
        public IEnumerable<AuditLogItemViewModel> Logs { get; set; } = new List<AuditLogItemViewModel>();
    }
    public class AuditLogItemViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserEmail { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
    }
}