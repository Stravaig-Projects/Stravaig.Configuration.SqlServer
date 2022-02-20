using Stravaig.Configuration.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddSqlServer(opts =>
{
    opts.FromExistingConfiguration()
        .ExpectLogger();
});

builder.Logging.AddConsole();

// Add services to the container.
var services = builder.Services;
services.AddControllersWithViews();
services.AddSingleton<IConfigurationRoot>(builder.Configuration);

var app = builder.Build();

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
builder.Configuration.AttachLoggerToSqlServerProvider(loggerFactory);

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();