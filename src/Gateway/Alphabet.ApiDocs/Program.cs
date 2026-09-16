using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DocsConsole",
        policy => policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true));
});

builder.Services.AddHttpClient("OpenApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Docs:OpenApiBaseUrl"] ?? "https://localhost:58241");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("DocsConsole");

app.MapGet("/openapi/{document}.json", async (
    string document,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    var client = httpClientFactory.CreateClient("OpenApi");
    using var response = await client.GetAsync($"/swagger/{document}/swagger.json", cancellationToken);
    var content = await response.Content.ReadAsStringAsync(cancellationToken);

    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

app.MapGet("/openapi/modules.json", () => Results.Json(new
{
    modules = new[]
    {
        new { name = "Authentication", slug = "authentication", spec = "/openapi/v1.json", tag = "Identity" },
        new { name = "Productivity", slug = "productivity", spec = "/openapi/v1.json", tag = "Productivity" },
        new { name = "Scheduler", slug = "scheduler", spec = "/openapi/v1.json", tag = "Scheduler" },
        new { name = "Privilege", slug = "privilege", spec = "/openapi/v1.json", tag = "Privilege" },
        new { name = "Asset Management", slug = "asset-management", spec = "/openapi/v1.json", tag = "AssetManagement" },
        new { name = "Leave Management", slug = "leave-management", spec = "/openapi/v1.json", tag = "LeaveManagement" }
    }
}));

app.MapReverseProxy();
app.MapFallbackToFile("index.html");

app.Run();
