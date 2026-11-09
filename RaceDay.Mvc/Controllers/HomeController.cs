using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Services; using RaceDay.Mvc.Models;
namespace RaceDay.Mvc.Controllers;
public class HomeController(RaceDayApiClient api):Controller {
 public async Task<IActionResult> Index()=>View(await api.Get<List<EventDto>>("api/events")??[]);
 public IActionResult Error()=>View();
}
