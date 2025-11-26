using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ForumClient.Services; 

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(o => o.DetailedErrors = true);

// ������������ � ������ API (���� ����� ����������!)
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7183/") // ���� �� launchSettings.json
});

// ������������ ������
builder.Services.AddScoped<ForumService>();

// ����������� ������ Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();