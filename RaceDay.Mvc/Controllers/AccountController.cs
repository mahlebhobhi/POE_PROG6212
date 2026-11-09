using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class AccountController(RaceDayApiClient api):Controller {
 [HttpGet] public IActionResult Login()=>View(new LoginVm());
 [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginVm vm){if(!ModelState.IsValid)return View(vm);try{await api.Login(vm.Email,vm.Password);return RedirectToAction("Index",ControllerFor(HttpContext.Session.GetString("UserRole")));}catch(ApiException e){ModelState.AddModelError("",e.Message);return View(vm);}}
 [HttpGet] public IActionResult Register()=>View(new RegisterVm());
 [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterVm vm){if(!ModelState.IsValid)return View(vm);try{await api.Register(vm.FullName,vm.Email,vm.Password,vm.Role);TempData["Success"]="Account created. Please sign in.";return RedirectToAction(nameof(Login));}catch(ApiException e){ModelState.AddModelError("",e.Message);return View(vm);}}
 public async Task<IActionResult> Logout(){await api.Logout();return RedirectToAction("Index","Home");}
 private static string ControllerFor(string? role)=>role=="Organiser"?"Organiser":"Participant";
}
