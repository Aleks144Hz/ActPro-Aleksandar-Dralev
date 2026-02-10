using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController(IAuditDashboardService auditService) : Controller
    {
        //--- Audit Logs Dashboard ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = await auditService.GetAllLogsAsync();
            return View(logs);
        }
    }
}