using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
namespace RaceDay.Mvc.Services;
public class RaceDayApiClient(HttpClient http, IHttpContextAccessor accessor, IConfiguration config)
{
    private readonly JsonSerializerOptions json = new(JsonSerializerDefaults.Web);
    private HttpContext Context => accessor.HttpContext!;
    private string BaseUrl => config["Api:BaseUrl"] ?? "http://localhost:5080/";
    private HttpRequestMessage Request(HttpMethod method,string path) { var req=new HttpRequestMessage(method,new Uri(new Uri(BaseUrl),path)); var cookie=Context.Session.GetString("ApiCookie"); if(!string.IsNullOrWhiteSpace(cookie)) req.Headers.TryAddWithoutValidation("Cookie",cookie); return req; }
    private async Task<HttpResponseMessage> Send(HttpRequestMessage req) { var res=await http.SendAsync(req); if(res.Headers.TryGetValues("Set-Cookie",out var values)) { var cookie=string.Join("; ",values.Select(v=>v.Split(';')[0])); if(cookie.Length>0) Context.Session.SetString("ApiCookie",cookie); } return res; }
    public async Task<T?> Get<T>(string path) { using var r=Request(HttpMethod.Get,path); using var x=await Send(r); if(!x.IsSuccessStatusCode)return default; return await x.Content.ReadFromJsonAsync<T>(json); }
    public async Task<T?> Post<T>(string path,object data) { using var r=Request(HttpMethod.Post,path); r.Content=JsonContent.Create(data,options:json); using var x=await Send(r); if(!x.IsSuccessStatusCode) throw new ApiException(await Error(x)); return await x.Content.ReadFromJsonAsync<T>(json); }
    public async Task<T?> Put<T>(string path,object data) { using var r=Request(HttpMethod.Put,path); r.Content=JsonContent.Create(data,options:json); using var x=await Send(r); if(!x.IsSuccessStatusCode) throw new ApiException(await Error(x)); return await x.Content.ReadFromJsonAsync<T>(json); }
    public async Task Delete(string path) { using var r=Request(HttpMethod.Delete,path); using var x=await Send(r); if(!x.IsSuccessStatusCode) throw new ApiException(await Error(x)); }
    public async Task<T?> Upload<T>(string path,IFormFile file) { using var r=Request(HttpMethod.Post,path); using var form=new MultipartFormDataContent(); await using var stream=file.OpenReadStream(); using var content=new StreamContent(stream); content.Headers.ContentType=new MediaTypeHeaderValue(file.ContentType); form.Add(content,"file",Path.GetFileName(file.FileName)); r.Content=form; using var x=await Send(r); if(!x.IsSuccessStatusCode)throw new ApiException(await Error(x)); return await x.Content.ReadFromJsonAsync<T>(json); }
    public async Task Login(string email,string password) { using var r=new HttpRequestMessage(HttpMethod.Post,new Uri(new Uri(BaseUrl),"api/auth/login")){Content=JsonContent.Create(new{email,password},options:json)}; using var x=await Send(r); if(!x.IsSuccessStatusCode)throw new ApiException(await Error(x)); var user=await x.Content.ReadFromJsonAsync<UserDto>(json) ?? throw new ApiException("The API returned an empty login response."); Context.Session.SetInt32("UserId",user.Id); Context.Session.SetString("UserName",user.FullName); Context.Session.SetString("UserRole",user.Role); }
    public async Task Register(string name,string email,string password,string role) { using var r=new HttpRequestMessage(HttpMethod.Post,new Uri(new Uri(BaseUrl),"api/auth/register")){Content=JsonContent.Create(new{fullName=name,email,password,role},options:json)}; using var x=await Send(r); if(!x.IsSuccessStatusCode)throw new ApiException(await Error(x)); }
    public async Task Logout() { try { using var r=Request(HttpMethod.Post,"api/auth/logout"); using var x=await Send(r); } finally { Context.Session.Clear(); } }
    private static async Task<string> Error(HttpResponseMessage r) { var s=await r.Content.ReadAsStringAsync(); try { using var d=JsonDocument.Parse(s); return d.RootElement.TryGetProperty("message",out var m)?m.GetString()??$"API error {(int)r.StatusCode}":$"API error {(int)r.StatusCode}"; } catch{return $"API error {(int)r.StatusCode}: {s}";} }
}
public class ApiException(string message):Exception(message);
