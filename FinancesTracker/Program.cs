using FinancesTracker.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();        // obsługa stron Razor (_Host.cshtml)
builder.Services.AddControllers();       // obsługa Web API
builder.Services.AddHttpClient();        // HttpClient dla Client
builder.Services.AddServerSideBlazor();  // opcjonalne — jeśli używasz komponentów SSR

// Entity Framework + PostgreSQL
builder.Services.AddDbContext<FinancesTrackerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodaj CORS dla development (jeśli Client działa osobno w trybie dev)
if (builder.Environment.IsDevelopment()) {
  builder.Services.AddCors(options =>
  {
    options.AddDefaultPolicy(policy =>
    {
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
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();   // potrzebne dla plików z wwwroot
app.UseRouting();

app.MapControllers();

// Ta linia ładuje Twoją aplikację Blazor WebAssembly z Client
app.MapFallbackToPage("/_Host");

app.Run();
