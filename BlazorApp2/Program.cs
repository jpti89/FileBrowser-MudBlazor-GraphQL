using BlazorApp2.Server.Blazor;
using MudBlazor.Services;
using BlazorApp2.Server.GraphQlApi.GraphQuery;
using BlazorApp2.Server.Services.BuisnessLogic.Implementations;
using BlazorApp2.Server.Services.BuisnessLogic.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IFileSystemService, FileSystemService>();

builder.Services.AddMudServices();

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7175", "http://localhost:5007")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseCors("AllowBlazor");

app.MapGraphQL();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();