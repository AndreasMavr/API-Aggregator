using ApiAggregator.Auth;
using ApiAggregator.Background;
using ApiAggregator.External;
using ApiAggregator.Infrastructure;
using ApiAggregator.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WeatherOptions>(builder.Configuration.GetSection("OpenWeatherMap"));
builder.Services.Configure<NewsOptions>(builder.Configuration.GetSection("NewsApi"));
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection("GitHub"));
builder.Services.Configure<StatsOptions>(builder.Configuration.GetSection("Stats"));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<IStatsStore, StatsStore>();

builder.Services.AddHttpClient("OpenWeatherMap", c =>
{
    var opts = builder.Configuration.GetSection("OpenWeatherMap").Get<WeatherOptions>()!;
    c.BaseAddress = new Uri(opts.BaseUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler(sp => new TelemetryHandler(
    "OpenWeatherMap",
    sp.GetRequiredService<IStatsStore>(),
    sp.GetRequiredService<ILogger<TelemetryHandler>>()))
.AddPolicyHandler(Policies.BuildStandardPolicy());

builder.Services.AddHttpClient("NewsApi", c =>
{
    var opts = builder.Configuration.GetSection("NewsApi").Get<NewsOptions>()!;
    c.BaseAddress = new Uri(opts.BaseUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.DefaultRequestHeaders.UserAgent.ParseAdd("ApiAggregator/1.0 (+https://example.local)");

})
.AddHttpMessageHandler(sp => new TelemetryHandler(
    "NewsApi",
    sp.GetRequiredService<IStatsStore>(),
    sp.GetRequiredService<ILogger<TelemetryHandler>>()))
.AddPolicyHandler(Policies.BuildStandardPolicy());

builder.Services.AddHttpClient("GitHub", c =>
{
    var opts = builder.Configuration.GetSection("GitHub").Get<GitHubOptions>()!;
    c.BaseAddress = new Uri(opts.BaseUrl);
    c.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    c.DefaultRequestHeaders.Add("User-Agent", "ApiAggregator/1.0");
    if (!string.IsNullOrWhiteSpace(opts.Token))
        c.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opts.Token);
})
.AddHttpMessageHandler(sp => new TelemetryHandler(
    "GitHub",
    sp.GetRequiredService<IStatsStore>(),
    sp.GetRequiredService<ILogger<TelemetryHandler>>()))
.AddPolicyHandler(Policies.BuildStandardPolicy());

builder.Services.AddScoped<IWeatherClient, WeatherClient>();
builder.Services.AddScoped<INewsClient, NewsClient>();
builder.Services.AddScoped<IGitHubClient, GitHubClient>();

builder.Services.AddScoped<FilterSortService>();
builder.Services.AddScoped<AggregationService>();


if (builder.Configuration.GetSection("Stats").Exists())
{
    builder.Services.AddHostedService<PerformanceMonitorService>();
}

var authOpts = builder.Configuration.GetSection("Auth").Get<AuthOptions>() ?? new AuthOptions();
if (authOpts.Enabled)
{
    builder.Services.AddJwtAuth(authOpts);
}

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "API Aggregator", Version = "v1" });

    if (authOpts.Enabled)
    {
        o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme
        });
        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

if (authOpts.Enabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();

app.Run();
