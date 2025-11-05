using System.ComponentModel.DataAnnotations;
using LetsServiceBus.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LetsServiceBus.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IServiceBusService _serviceBusService;

    [BindProperty]
    [Required(ErrorMessage = "Please enter a comment")]
    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string Comment { get; set; } = string.Empty;

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IServiceBusService serviceBusService)
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
            await _serviceBusService.SendMessageAsync(Comment);
            
            _logger.LogInformation("Comment successfully sent to Service Bus: {Comment}", Comment);
            
            SuccessMessage = "Your comment has been successfully sent to the Service Bus queue!";
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending comment to Service Bus");
            
            ErrorMessage = $"Failed to send comment: {ex.Message}";
            
            return Page();
        }
    }
}
