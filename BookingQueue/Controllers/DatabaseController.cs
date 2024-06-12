using Microsoft.AspNetCore.Mvc;

namespace BookingQueue.Controllers;

public class DatabaseController : Controller
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DatabaseController(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult SwitchDatabase(string databaseName)
    {
        _httpContextAccessor.HttpContext!.Session.SetString("SelectedDatabase", databaseName);
        return RedirectToAction("Index", "Home");
    }
}