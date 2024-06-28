using System.Security.Claims;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace BookingQueue.Controllers;

public class AuthController : Controller
{
    private readonly IUserRepository _repository;

    public AuthController(IUserRepository repository)
    {
        _repository = repository;
    }
    
    [HttpGet("/admin/login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("/admin/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _repository.AuthenticateAsync(username, password);
        
        if(user is not null){
            // Set authentication cookie or token here
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new("Role", user.Roles.First().Name) // Assumes single role
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Login");
            await HttpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

            // Redirect based on role
            if (user.Roles.Any(r => r.Name == RoleConstants.SuperAdmin))
            {
                return RedirectToAction("SuperAdminPage", "SuperAdmin");
            }
        }
        
        ModelState.AddModelError("", "Invalid username or password");
        
        return View();
    }

    [HttpGet("/admin/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login", "Auth");
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}