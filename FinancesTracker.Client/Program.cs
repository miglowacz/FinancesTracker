using FinancesTracker.Client;
using FinancesTracker.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Konfiguracja HttpClient z bazowym adresem
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Rejestracja serwisów
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<cCategoryService>();
builder.Services.AddScoped<CategoryRuleService>();

await builder.Build().RunAsync();
