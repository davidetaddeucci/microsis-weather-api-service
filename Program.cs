using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using WeatherAPI.Services;
using WeatherAPI.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Aggiungi la configurazione delle opzioni
builder.Services.Configure<WeatherApiOptions>(
    builder.Configuration.GetSection(WeatherApiOptions.SectionName));

// Aggiungi servizi al container
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IWeatherServiceExtended, WeatherServiceExtended>();

// Registra HttpClient con policy di resilienza
builder.Services.AddHttpClient<IWeatherServiceExtended, WeatherServiceExtended>((provider, client) => {
    var options = provider.GetRequiredService<IOptions<WeatherApiOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
.AddPolicyHandler(GetRetryPolicy(builder.Configuration));

// Aggiungi controller
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Aggiungi API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Aggiungi Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Servizio Previsioni Meteo API", 
        Version = "v1",
        Description = "API per le previsioni meteorologiche utilizzando WeatherAPI.com",
        Contact = new OpenApiContact
        {
            Name = "Microsis",
            Email = "info@microsis.com",
            Url = new Uri("https://microsis.com")
        }
    });
    
    // Aggiungi commenti XML per la documentazione
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    c.EnableAnnotations();
});

// Aggiungi CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Aggiungi Response Caching
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB
    options.SizeLimit = 100 * 1024 * 1024; // 100MB
});

// Aggiungi Distributed Caching per migliorare prestazioni con cache delle richieste all'API
builder.Services.AddDistributedMemoryCache();

// Aggiungi logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configura la pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1");
        c.RoutePrefix = "swagger";
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseResponseCaching();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Metodo per configurare policy di resilienza HTTP
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IConfiguration configuration)
{
    var options = configuration.GetSection(WeatherApiOptions.SectionName).Get<WeatherApiOptions>() 
        ?? new WeatherApiOptions();
    
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            options.MaxRetries, 
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        );
}