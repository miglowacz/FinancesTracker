using FinancesTracker.Client;
using FinancesTracker.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Rejestracja serwisów
builder.Services.AddScoped<cApiService>();
builder.Services.AddScoped<cAccountService>();
builder.Services.AddScoped<cCategoryService>();
builder.Services.AddScoped<cSubcategoryService>();
builder.Services.AddScoped<cTransactionService>();
builder.Services.AddScoped<cTransactionImportService>();
builder.Services.AddScoped<cCategoryRuleService>();
builder.Services.AddScoped<cAccountRuleService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
