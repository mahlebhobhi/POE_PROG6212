using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ProfileController(RaceDayApiClient api):Controller {
 private bool Allowed()=>HttpContext.Session.GetInt32("UserId")!=null; private IActionResult Denied()=>RedirectToAction("Login","Account");
 [HttpGet] public async Task<IActionResult> Index(){if(!Allowed())return Denied();var u=await api.Get<UserDto>("api/users/me");if(u==null)return Denied();return View(new ProfileVm{FullName=u.FullName,Email=u.Email,ProfileImageUrl=u.ProfileImageUrl});}
 [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Index(ProfileVm vm){if(!Allowed())return Denied();if(!ModelState.IsValid)return View(vm);try{await api.Put<UserDto>("api/users/me",new{fullName=vm.FullName,email=vm.Email});HttpContext.Session.SetString("UserName",vm.FullName);if(vm.Picture!=null)await api.Upload<object>("api/users/me/picture",vm.Picture);TempData["Success"]="Profile updated.";return RedirectToAction(nameof(Index));}catch(ApiException e){ModelState.AddModelError("",e.Message);return View(vm);}}
}
