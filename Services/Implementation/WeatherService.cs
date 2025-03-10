using System.Text.Json;
using Microsoft.Extensions.Options;
using WeatherAPI.Models;
using WeatherAPI.Services;

namespace WeatherAPI.Services.Implementation;

/// <summary>
/// Implementazione del servizio per le previsioni meteorologiche che utilizza WeatherAPI.com
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiOptions _options;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configura l'URL base per l'API esterna
        _httpClient.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
    }

    /// <inheritdoc />
    public async Task<WeatherForecastDto> GetCurrentWeatherAsync(string location)
    {
        try
        {
            // Costruisci l'URL con i parametri necessari
            var requestUri = $"current.json?key={_options.ApiKey}&q={Uri.EscapeDataString(location)}&aqi=no";
            
            // Effettua la richiesta HTTP
            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            // Deserializza la risposta
            var content = await response.Content.ReadAsStreamAsync();
            var weatherData = await JsonSerializer.DeserializeAsync<WeatherForecastResponse>(content);
            
            if (weatherData == null)
                throw new JsonException("Impossibile deserializzare i dati meteo");
            
            // Converti i dati nel DTO
            return MapToWeatherForecastDto(weatherData);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Errore durante la richiesta delle previsioni meteo correnti per {Location}", location);
            throw new ApplicationException($"Impossibile ottenere le previsioni meteo per {location}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'elaborazione delle previsioni meteo per {Location}", location);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<WeatherForecastDto> GetForecastAsync(WeatherRequest request)
    {
        try
        {
            // Controlla i parametri di input
            if (string.IsNullOrWhiteSpace(request.Location))
                throw new ArgumentException("La località è obbligatoria", nameof(request.Location));
            
            if (request.Days <= 0 || request.Days > 10)
                request.Days = 3; // Usa un valore predefinito se fuori intervallo
            
            // Costruisci l'URL con i parametri necessari
            var requestUri = $"forecast.json?key={_options.ApiKey}" +
                            $"&q={Uri.EscapeDataString(request.Location)}" +
                            $"&days={request.Days}" +
                            $"&aqi={(request.AirQuality ? "yes" : "no")}" +
                            $"&alerts={(request.Alerts ? "yes" : "no")}";
            
            // Effettua la richiesta HTTP
            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            // Deserializza la risposta
            var content = await response.Content.ReadAsStreamAsync();
            var weatherData = await JsonSerializer.DeserializeAsync<WeatherForecastResponse>(content);
            
            if (weatherData == null)
                throw new JsonException("Impossibile deserializzare i dati meteo");
            
            // Converti i dati nel DTO
            return MapToWeatherForecastDto(weatherData);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Errore durante la richiesta delle previsioni meteo per {Location}", request.Location);
            throw new ApplicationException($"Impossibile ottenere le previsioni meteo per {request.Location}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'elaborazione delle previsioni meteo per {Location}", request.Location);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> SearchLocationsAsync(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
                return Enumerable.Empty<string>();
            
            // Costruisci l'URL per la ricerca delle località
            var requestUri = $"search.json?key={_options.ApiKey}&q={Uri.EscapeDataString(query)}";
            
            // Effettua la richiesta HTTP
            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            
            // Deserializza la risposta come array di location
            var content = await response.Content.ReadAsStreamAsync();
            var locations = await JsonSerializer.DeserializeAsync<List<Location>>(content);
            
            if (locations == null)
                return Enumerable.Empty<string>();
            
            // Restituisci i nomi delle località
            return locations.Select(l => $"{l.Name}, {l.Region}, {l.Country}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Errore durante la ricerca delle località per la query {Query}", query);
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'elaborazione della ricerca località per {Query}", query);
            return Enumerable.Empty<string>();
        }
    }
    
    #region Helper Methods
    
    protected WeatherForecastDto MapToWeatherForecastDto(WeatherForecastResponse response)
    {
        var result = new WeatherForecastDto
        {
            Location = response.Location.Name,
            Country = response.Location.Country,
            LastUpdated = ParseDateTime(response.Current.LastUpdated),
            CurrentTemperature = response.Current.TemperatureC,
            Condition = response.Current.Condition.Text,
            ConditionIcon = FixIconUrl(response.Current.Condition.Icon),
            Humidity = response.Current.Humidity,
            WindSpeed = response.Current.WindKph,
            WindDirection = response.Current.WindDirection
        };
        
        // Aggiungi le previsioni giornaliere se disponibili
        if (response.Forecast?.ForecastDays != null)
        {
            result.DailyForecasts = response.Forecast.ForecastDays.Select(fd => new DailyForecastDto
            {
                Date = DateTime.Parse(fd.Date),
                MaxTemp = fd.Day.MaxTempC,
                MinTemp = fd.Day.MinTempC,
                AvgTemp = fd.Day.AvgTempC,
                Condition = fd.Day.Condition.Text,
                ConditionIcon = FixIconUrl(fd.Day.Condition.Icon),
                ChanceOfRain = fd.Day.ChanceOfRain,
                Sunrise = fd.Astro.Sunrise,
                Sunset = fd.Astro.Sunset
            }).ToList();
        }
        
        return result;
    }
    
    protected DateTime ParseDateTime(string dateTimeStr)
    {
        return DateTime.TryParse(dateTimeStr, out var result) 
            ? result 
            : DateTime.Now;
    }
    
    protected string FixIconUrl(string iconUrl)
    {
        // WeatherAPI restituisce URL relativi per le icone, quindi aggiungiamo l'URL di base se necessario
        if (string.IsNullOrEmpty(iconUrl))
            return string.Empty;
            
        return iconUrl.StartsWith("//") 
            ? $"https:{iconUrl}" 
            : iconUrl;
    }
    
    #endregion
    
    // Proprietà protette per l'accesso dalle classi derivate
    protected HttpClient HttpClient => _httpClient;
    protected string ApiKey => _options.ApiKey;
    protected ILogger Logger => _logger;
}

/// <summary>
/// Opzioni di configurazione per il servizio WeatherAPI
/// </summary>
public class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";
    
    /// <summary>
    /// Chiave API per WeatherAPI.com
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Timeout in secondi per le richieste HTTP (default: 30 secondi)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Numero massimo di tentativi per le richieste HTTP (default: 3)
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
