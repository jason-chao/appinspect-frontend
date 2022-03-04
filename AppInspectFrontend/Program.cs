using AppInspectFrontend;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string? rpcUrl = builder.Configuration["AppInspectRPCUrl"];
string? apiBaseUrl = builder.Configuration["AppInspectAPIBaseUrl"];

Uri? rpcUri = null;

if (!string.IsNullOrEmpty(rpcUrl))
    rpcUri = new Uri(rpcUrl);
else
    rpcUri = new Uri(new Uri(builder.HostEnvironment.BaseAddress), "rpc");

if (string.IsNullOrEmpty(apiBaseUrl))
    apiBaseUrl = builder.HostEnvironment.BaseAddress;

builder.Services.AddTransient(to => new FrontendSettings() { AppInspectRPCUrl = rpcUri, AppInspectAPIUrl = new Uri(apiBaseUrl) });

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

var app = builder.Build();

await app.RunAsync();
