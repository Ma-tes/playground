using Microsoft.AspNetCore.Authentication.Cookies;
using ShareCar.WebClient.App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
  });

var backendUrl = builder.Configuration["BackendApi:BaseUrl"] ?? "https://localhost:7153";
builder.Services.AddHttpClient<BackendApiClient>(client =>
{
  client.BaseAddress = new Uri(backendUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Dashboard/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
