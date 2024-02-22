using Microsoft.EntityFrameworkCore;
using RugbyWatch.Components;
using RugbyWatch.Data;
using RugbyWatch.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();

builder.Services.AddScoped<StoreMatchReportService>();
builder.Services.AddScoped<LineupComplianceCheckService>();
builder.Services.AddScoped<DownloadMatchReportService>();
builder.Services.AddScoped<ProcessMatchReportsService>();
builder.Services.AddScoped<GetSuspiciousMatchesService>();
builder.Services.AddScoped<GetMatchInformationService>();

builder.Services.AddDbContext<RugbyMatchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RugbyWatchDb")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if ( !app.Environment.IsDevelopment() ) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
