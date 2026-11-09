using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController(RaceDayApiClient api):Controller {
 private bool Allowed()=>HttpContext.Session.GetString("UserRole")=="Participant"&&HttpContext.Session.GetInt32("UserId")!=null; private IActionResult Denied()=>RedirectToAction("Login","Account");
 public async Task<IActionResult> Index(string? q,string? type){if(!Allowed())return Denied();var events=await api.Get<List<EventDto>>("api/events")??[];if(!string.IsNullOrWhiteSpace(q))events=events.Where(e=>e.Name.Contains(q,StringComparison.OrdinalIgnoreCase)||e.Location.Contains(q,StringComparison.OrdinalIgnoreCase)).ToList();if(!string.IsNullOrWhiteSpace(type))events=events.Where(e=>e.EventType==type).ToList();ViewBag.Query=q;ViewBag.Type=type;return View(events);}
 public async Task<IActionResult> Details(int id){if(!Allowed())return Denied();var e=await api.Get<EventDto>($"api/events/{id}");if(e==null)return NotFound();ViewBag.Categories=await api.Get<List<CategoryDto>>($"api/events/{id}/categories")??[];return View(e);}
 [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Enrol(int eventId,int categoryId){if(!Allowed())return Denied();try{await api.Post<object>($"api/enrollments/events/{eventId}",new{categoryId});TempData["Success"]="You have successfully enrolled.";}catch(ApiException e){TempData["Error"]=e.Message;}return RedirectToAction(nameof(Details),new{id=eventId});}
 public async Task<IActionResult> MyEvents(){if(!Allowed())return Denied();return View(await api.Get<List<EnrollmentDto>>("api/enrollments/mine")??[]);}
 public async Task<IActionResult> MyResults(){if(!Allowed())return Denied();return View(await api.Get<List<ResultDto>>("api/results/mine")??[]);}
}
