var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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


