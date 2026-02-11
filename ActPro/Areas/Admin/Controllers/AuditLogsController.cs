using ActPro.Domain.Models.Areas;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController(IAuditDashboardService auditService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = await auditService.GetAuditLogsIndexModelAsync();
            return View(viewModel);
        }
    }
}