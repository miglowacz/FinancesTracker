using FinancesTracker.Client;
using FinancesTracker.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

//konfiguracja HttpClient z bazowym adresem
builder.Services.AddScoped(sp => new HttpClient { 
  BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

//rejestracja serwisów (usuń duplikaty AddHttpClient)
builder.Services.AddHttpClient<cApiService>(client => {
  client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddScoped<cTransactionService>();
builder.Services.AddScoped<cCategoryService>();
builder.Services.AddScoped<cAccountService>();

builder.Services.AddHttpClient<cSubcategoryService>(client => {
  client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
builder.Services.AddHttpClient<cCategoryRuleService>(client => {
  client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
builder.Services.AddHttpClient<cTransactionImportService>(client => {
  client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddMudServices();

await builder.Build().RunAsync();
