using _10_project_webiste.Data;
using _10_project_webiste.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddScoped<IStudyPlannerService, StudyPlannerService>();
var neonConnectionString = builder.Configuration.GetConnectionString("Neon");
builder.Services.AddDbContext<StudyDbContext>(options =>
    options.UseNpgsql(
        neonConnectionString 
        ?? "Host=localhost;Database=study_sprint;Username=postgres;Password=not-configured"
        ));

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
