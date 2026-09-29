using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace InventoryERP.Web.Services.Ai;

/// <summary>
/// Talks to a locally-running Ollama server using function/tool calling: the model decides which
/// AiDataToolsService method(s) it needs to answer the question, we run them against the real
/// database, feed the results back, and let the model compose the final natural-language reply.
/// This means answers always reflect live data instead of anything "learned" at training time.
/// </summary>
public class AiAssistantService : IAiAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly AiDataToolsService _tools;
    private readonly string _model;

    public AiAssistantService(HttpClient httpClient, AiDataToolsService tools, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _tools = tools;
        _model = configuration["Ollama:Model"] ?? "llama3.1";
    }

    public async Task<string> AskAsync(string userMessage, List<ChatTurn> history)
    {
        var messages = new List<OllamaMessage> { new() { role = "system", content = BuildSystemPrompt() } };
        foreach (var turn in history.TakeLast(12))
            messages.Add(new OllamaMessage { role = turn.Role, content = turn.Content });
        messages.Add(new OllamaMessage { role = "user", content = userMessage });

        for (var iteration = 0; iteration < 5; iteration++)
        {
            OllamaChatResponse? response;
            try
            {
                var httpResponse = await _httpClient.PostAsJsonAsync("api/chat", new OllamaChatRequest
                {
                    model = _model,
                    messages = messages,
                    tools = BuildToolSchema(),
                    stream = false,
                });
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadFromJsonAsync<OllamaChatResponse>();
            }
            catch (HttpRequestException)
            {
                return "AI assistant abhi available nahi hai (Ollama service se connect nahi ho saka). " +
                       "Please make sure Ollama is installed and running, and the model is pulled - e.g. run: ollama pull llama3.1, then ollama serve.\n\n" +
                       "AI assistant is currently unavailable - please make sure Ollama is running.";
            }

            if (response?.message == null)
                return "AI se koi jawab nahi mila. Please try again.";

            if (response.message.tool_calls is { Count: > 0 } calls)
            {
                messages.Add(response.message);
                foreach (var call in calls)
                {
                    var result = await ExecuteToolAsync(call.function.name, call.function.arguments);
                    messages.Add(new OllamaMessage { role = "tool", content = result });
                }
                continue;
            }

            return response.message.content ?? "";
        }

        return "Maaf kijiye, ye sawal process karne mein zyada steps lag rahe hain. Please dobara, saada sawal poochein.";
    }

    private async Task<string> ExecuteToolAsync(string name, JsonElement args)
    {
        try
        {
            var today = DateTime.Now.Date;
            object result = name switch
            {
                "sales_summary" => await _tools.SalesSummaryAsync(GetDate(args, "from", today), GetDate(args, "to", today)),
                "purchases_summary" => await _tools.PurchasesSummaryAsync(GetDate(args, "from", today), GetDate(args, "to", today)),
                "top_selling_products" => await _tools.TopSellingProductsAsync(GetDate(args, "from", today), GetDate(args, "to", today), GetInt(args, "limit", 5)),
                "low_stock_products" => await _tools.LowStockProductsAsync(GetInt(args, "limit", 20)),
                "inventory_overview" => await _tools.InventoryOverviewAsync(),
                "recent_activity" => await _tools.RecentActivityAsync(GetInt(args, "limit", 10)),
                _ => new { error = $"Unknown tool '{name}'" },
            };
            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private static DateTime GetDate(JsonElement args, string prop, DateTime fallback)
    {
        if (args.ValueKind == JsonValueKind.Object && args.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            && DateTime.TryParse(v.GetString(), out var parsed))
            return parsed;
        return fallback;
    }

    private static int GetInt(JsonElement args, string prop, int fallback)
    {
        if (args.ValueKind != JsonValueKind.Object || !args.TryGetProperty(prop, out var v)) return fallback;
        if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n)) return n;
        if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var n2)) return n2;
        return fallback;
    }

    private static string BuildSystemPrompt() =>
        $"""
         You are the AI Assistant embedded inside an Inventory ERP web application.
         Today's date is {DateTime.Now:yyyy-MM-dd} ({DateTime.Now:dddd}), current time {DateTime.Now:HH:mm}.

         Rules:
         - You must ONLY answer using data returned by the provided tools. Never guess or invent numbers.
         - Always call the relevant tool(s) first before answering any question about sales, purchases, customers, stock, products or recent activity.
         - When the user says "today", "aaj", "is hafte", "is month", "yesterday" etc, compute the correct from/to dates yourself (format yyyy-MM-dd) based on today's date above and pass them to the tools.
         - Reply in the SAME language and script the user used: if they typed in English, reply in English. If they typed in Urdu script, reply in Urdu script. If they typed Roman Urdu (Urdu words spelled with English letters, e.g. "aaj kitni sale hui"), reply in Roman Urdu the same way. Match their style.
         - Be concise and conversational. Use the real numbers from the tool results. Format money plainly (e.g. "Rs. 12,500").
         - If asked something unrelated to this business/inventory data, politely say (in the user's language) that you can only help with questions about this store's sales, purchases, stock, customers and activity.
         """;

    private static List<object> BuildToolSchema() => new()
    {
        new
        {
            type = "function",
            function = new
            {
                name = "sales_summary",
                description = "Get total sales orders, revenue and distinct customers for a date range.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        from = new { type = "string", description = "Start date, format yyyy-MM-dd" },
                        to = new { type = "string", description = "End date, format yyyy-MM-dd" },
                    },
                    required = Array.Empty<string>(),
                },
            },
        },
        new
        {
            type = "function",
            function = new
            {
                name = "purchases_summary",
                description = "Get total purchase orders, amount spent and distinct suppliers for a date range.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        from = new { type = "string", description = "Start date, format yyyy-MM-dd" },
                        to = new { type = "string", description = "End date, format yyyy-MM-dd" },
                    },
                    required = Array.Empty<string>(),
                },
            },
        },
        new
        {
            type = "function",
            function = new
            {
                name = "top_selling_products",
                description = "Get the best-selling products by quantity sold within a date range.",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        from = new { type = "string", description = "Start date, format yyyy-MM-dd" },
                        to = new { type = "string", description = "End date, format yyyy-MM-dd" },
                        limit = new { type = "integer", description = "Max number of products to return, default 5" },
                    },
                    required = Array.Empty<string>(),
                },
            },
        },
        new
        {
            type = "function",
            function = new
            {
                name = "low_stock_products",
                description = "Get products that are at or below their reorder level (low stock alerts).",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        limit = new { type = "integer", description = "Max number of products to return, default 20" },
                    },
                    required = Array.Empty<string>(),
                },
            },
        },
        new
        {
            type = "function",
            function = new
            {
                name = "inventory_overview",
                description = "Get overall counts: total products, categories, suppliers, customers, total stock value, and low stock count.",
                parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() },
            },
        },
        new
        {
            type = "function",
            function = new
            {
                name = "recent_activity",
                description = "Get the most recent user actions recorded in the system's activity log (who did what and when).",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        limit = new { type = "integer", description = "Max number of recent activity entries to return, default 10" },
                    },
                    required = Array.Empty<string>(),
                },
            },
        },
    };
}
