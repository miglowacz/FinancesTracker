using FinancesTracker.Client;
using FinancesTracker.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Konfiguracja HttpClient z bazowym adresem
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Rejestracja serwisów
builder.Services.AddHttpClient<ApiService>(client => { client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress); });

builder.Services.AddScoped<cTransactionService>();
builder.Services.AddHttpClient<ApiService>(client => {
  client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
builder.Services.AddScoped<cCategoryService>();

builder.Services.AddHttpClient<cSubcategoryService>(client => {
  client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
builder.Services.AddHttpClient<cCategoryRuleService>(client => {
  client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

await builder.Build().RunAsync();
