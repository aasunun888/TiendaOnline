using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie(options =>
{
    options.Cookie.Name = "LogAuthCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict; //Acepta que solo sean solicitudes de mi sitio web
    options.ExpireTimeSpan = TimeSpan.FromDays(1); // Tiempo hasta que se cierre la sesion
    options.SlidingExpiration = true; //Tiempo de inactividad siempre cuando acabe un dia
    options.LoginPath = "/auth/LoginView";
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = (context) =>
        {
            context.Response.Redirect("/auth/LoginView");
            return Task.CompletedTask;

        }

    };

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//Ruta administracion
app.MapAreaControllerRoute(
    name: "Administracion",
    pattern: "administracion/{controller=Home}/{action=Index}/{id?}",
    areaName: "Administracion"
    );


//Ruta publica
app.MapAreaControllerRoute(
    name: "Publica",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    areaName: "Publica"
    );

app.Run();


