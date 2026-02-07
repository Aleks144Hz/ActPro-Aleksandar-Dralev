using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditDashboardService _auditService;

        public AuditLogsController(IAuditDashboardService auditService)
        {
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _auditService.GetAllLogsAsync();
            return View(logs);
        }
    }
}