using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => { o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Strict; o.ExpireTimeSpan = TimeSpan.FromHours(8); o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = 401; return Task.CompletedTask; }; });
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath,"App_Data","keys")));
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter("login", l => { l.PermitLimit=10; l.Window=TimeSpan.FromMinutes(1); l.QueueLimit=0; }));
var app = builder.Build();
app.UseRateLimiter(); app.UseDefaultFiles(); app.UseStaticFiles(); app.UseAuthentication(); app.UseAuthorization();
var gate = new object();
var directory = Path.Combine(app.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(directory);
var file = Path.Combine(directory, "state.json");
var state = File.Exists(file) ? JsonSerializer.Deserialize<Storage>(File.ReadAllText(file))! : new Storage { Bins = [new(1,"Tacho principal","Entrada principal",92)] };
void Save() { var temp = file + ".tmp"; File.WriteAllText(temp,JsonSerializer.Serialize(state)); File.Move(temp,file,true); }
if (state.Bins.RemoveAll(bin => bin.Id != 1) > 0) Save();
bool Equal(string a,string b) => CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(a)),SHA256.HashData(Encoding.UTF8.GetBytes(b)));
app.MapPost("/api/login",async (Login input,HttpContext ctx) => {
 var password = builder.Configuration["ADMIN_PASSWORD"] ?? (app.Environment.IsDevelopment() ? "Admin123!" : null);
 if(password is null) return Results.Problem("Configura ADMIN_PASSWORD para iniciar sesión.",statusCode:503);
 if(!Equal(input.User ?? "",builder.Configuration["ADMIN_USER"] ?? "admin") || !Equal(input.Password ?? "",password)) return Results.Unauthorized();
 await ctx.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name,input.User!)],CookieAuthenticationDefaults.AuthenticationScheme))); return Results.Ok();
}).RequireRateLimiting("login");
app.MapPost("/api/logout",async (HttpContext c) => { await c.SignOutAsync(); return Results.Ok(); }).RequireAuthorization();
app.MapGet("/api/state",() => { lock(gate) return Results.Json(state); }).RequireAuthorization();
app.MapPost("/api/bins/{id:int}/clean",(int id,HttpContext context) => { lock(gate) { var bin=state.Bins.Find(b=>b.Id==id); if(bin is null) return Results.NotFound(); if(bin.Level==0) return Results.Conflict(new{message="El tacho ya está limpio."}); state.History.Insert(0,new(bin.Name,bin.Location,bin.Level,DateTimeOffset.UtcNow,context.User.Identity!.Name!)); bin.Level=0; Save(); return Results.Ok(); } }).RequireAuthorization();
app.Run();
record Login(string? User,string? Password);
class Storage { public List<Bin> Bins {get;set;}=[]; public List<Cleaning> History {get;set;}=[]; }
class Bin(int id,string name,string location,int level) { public int Id {get;set;}=id; public string Name {get;set;}=name; public string Location {get;set;}=location; public int Level {get;set;}=level; }
record Cleaning(string Name,string Location,int PreviousLevel,DateTimeOffset Date,string User);
