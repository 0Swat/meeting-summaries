using Microsoft.EntityFrameworkCore;
using MeetingSummaries.Api.Components;
using MeetingSummaries.Api.Data;
using MeetingSummaries.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// API
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new()
    {
        Title = "Meeting Summaries API",
        Version = "v1",
        Description = "API do zarządzania podsumowaniami spotkań zespołowych (Daily, Refinement, Retro, Sprint Review, Sprint Planning)."
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    o.IncludeXmlComments(xmlPath);
});

// Services
builder.Services.AddScoped<MeetingService>();

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient for Blazor pages calling own API
builder.Services.AddHttpClient("self", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SelfBaseUrl"] ?? "http://localhost:8080/");
});
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("self"));

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
