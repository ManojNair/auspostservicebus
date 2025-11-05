using LetsServiceBus.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LetsServiceBus.Pages;

public class ViewMessagesModel : PageModel
{
    private readonly ILogger<ViewMessagesModel> _logger;
    private readonly IServiceBusService _serviceBusService;

    [BindProperty]
    public int MaxMessages { get; set; } = 10;

    public List<string>? Messages { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public ViewMessagesModel(ILogger<ViewMessagesModel> logger, IServiceBusService serviceBusService)
    {
        _logger = logger;
        _serviceBusService = serviceBusService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            Messages = await _serviceBusService.ReceiveMessagesAsync(MaxMessages);
            
            _logger.LogInformation("Retrieved {Count} messages from Service Bus", Messages.Count);
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages from Service Bus");
            
            ErrorMessage = $"Failed to retrieve messages: {ex.Message}";
            
            return Page();
        }
    }
}
