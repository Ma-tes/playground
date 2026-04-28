using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ShareCar.WebClient.App.Models;
using ShareCar.WebClient.App.Services;

namespace ShareCar.WebClient.App.Controllers;

public class AccountController : Controller
{
  private readonly BackendApiClient _apiClient;

  public AccountController(BackendApiClient apiClient)
  {
    _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
  }

  [HttpGet]
  public IActionResult Login(string? returnUrl = null)
  {
    ViewData["ReturnUrl"] = returnUrl;

    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
  {
    ViewData["ReturnUrl"] = returnUrl;

    if (!ModelState.IsValid)
    {
      return View(model);
    }

    var token = await _apiClient.LoginAsync(model.Username, model.Password);

    if (token is null)
    {
      ModelState.AddModelError(string.Empty, "Invalid username or password.");

      return View(model);
    }

    var claims = new List<Claim>
    {
      new(ClaimTypes.Name, model.Username),
      new("jwt", token)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return LocalRedirect(returnUrl ?? Url.Action("Index", "Dashboard")!);
  }

  [HttpGet]
  public IActionResult Register()
  {
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Register(RegisterViewModel model)
  {
    if (!ModelState.IsValid)
    {
      return View(model);
    }

    var success = await _apiClient.RegisterAsync(model.Username, model.Password, model.Email);
    if (!success)
    {
      ModelState.AddModelError(string.Empty, "Registration failed. Username may already be taken.");

      return View(model);
    }

    TempData["Success"] = "Account created. You can now log in.";

    return RedirectToAction(nameof(Login));
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Logout()
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    return RedirectToAction(nameof(Login));
  }
}
