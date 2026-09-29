using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Services.Ai;

namespace InventoryERP.Web.Controllers;

[Authorize(Policy = Permissions.AiAssistant.View)]
public class AiAssistantController : Controller
{
    private readonly IAiAssistantService _aiAssistant;

    public AiAssistantController(IAiAssistantService aiAssistant) => _aiAssistant = aiAssistant;

    public class AskRequest
    {
        public string Message { get; set; } = string.Empty;
        public List<ChatTurn> History { get; set; } = new();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message)) return BadRequest();

        var reply = await _aiAssistant.AskAsync(request.Message.Trim(), request.History ?? new List<ChatTurn>());
        return Json(new { reply });
    }
}
