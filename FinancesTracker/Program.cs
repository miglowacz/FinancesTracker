using FinancesTracker.Data;
using FinancesTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//rejestracja kontrolerów API
builder.Services.AddControllers();
builder.Services.AddHttpClient();

//konfiguracja bazy danych z snake_case naming convention
builder.Services.AddDbContext<FinancesTrackerDbContext>(options =>
  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention());

//rejestracja serwisów
builder.Services.AddScoped<cAccountRuleService>();
builder.Services.AddScoped<cDataSeedService>();

//CORS dla developmentu
if (builder.Environment.IsDevelopment()) {
  builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
      policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
  });
}

var app = builder.Build();

//konfiguracja middleware
if (app.Environment.IsDevelopment()) {
  app.UseWebAssemblyDebugging();
  app.UseCors();
} else {
  app.UseExceptionHandler("/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();

//serwowanie plików Blazor WASM
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

//mapowanie kontrolerów API
app.MapControllers();

//fallback do index.html dla routingu po stronie klienta
app.MapFallbackToFile("index.html");

app.Run();
