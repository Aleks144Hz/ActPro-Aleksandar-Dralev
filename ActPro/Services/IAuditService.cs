namespace ActPro.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entity, string? entityId, string details);
    }
}