using FinancesTracker.Client.Pages;
using FinancesTracker.Components;
using FinancesTracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using FinancesTracker.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

//Dodaj Entity Framework
builder.Services.AddDbContext<FinancesTrackerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Dodaj kontrolery Web API
builder.Services.AddControllers();

//Dodaj HttpClient dla komunikacji Client-Server (factory)
builder.Services.AddHttpClient();

// Dodaj CORS dla development
if (builder.Environment.IsDevelopment()) {
  builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
      policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
  });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseWebAssemblyDebugging();
  app.UseCors();
} else {
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

// Mapuj kontrolery API
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FinancesTracker.Client._Imports).Assembly);

app.Run();
