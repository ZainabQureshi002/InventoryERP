namespace InventoryERP.Web.Services.Ai;

public interface IAiAssistantService
{
    Task<string> AskAsync(string userMessage, List<ChatTurn> history);
}
