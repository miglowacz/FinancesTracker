using FinancesTracker.Data;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<FinancesTrackerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

if (builder.Environment.IsDevelopment()) {
  builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
      policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
  });
}

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
  app.UseWebAssemblyDebugging();
  app.UseCors();
} else {
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();

// Serve Blazor client files and static assets
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html"); // ZMIENIONE: wskazuje na index.html z Client

app.Run();
