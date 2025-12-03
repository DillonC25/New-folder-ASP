using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Logout requested for user {User}", User?.Identity?.Name ?? "(anonymous)");
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Sign-out completed for user {User}", User?.Identity?.Name ?? "(anonymous)");
            return RedirectToAction("Index", "Home");
        }
}
