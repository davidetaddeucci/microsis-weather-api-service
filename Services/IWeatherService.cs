using WeatherAPI.Models;

namespace WeatherAPI.Services;

/// <summary>
/// Interfaccia per il servizio di previsioni meteorologiche
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Ottiene le previsioni del tempo correnti per una località specifica
    /// </summary>
    /// <param name="location">Nome della città o coordinate (lat,lon)</param>
    /// <returns>Dati meteo correnti</returns>
    Task<WeatherForecastDto> GetCurrentWeatherAsync(string location);
    
    /// <summary>
    /// Ottiene le previsioni del tempo per i giorni specificati
    /// </summary>
    /// <param name="request">Richiesta contenente località e parametri aggiuntivi</param>
    /// <returns>Previsioni meteo dettagliate</returns>
    Task<WeatherForecastDto> GetForecastAsync(WeatherRequest request);
    
    /// <summary>
    /// Cerca località in base al testo inserito
    /// </summary>
    /// <param name="query">Testo di ricerca</param>
    /// <returns>Lista di località corrispondenti</returns>
    Task<IEnumerable<string>> SearchLocationsAsync(string query);
}

/// <summary>
/// Interfaccia estesa del servizio meteo con funzionalità aggiuntive
/// </summary>
public interface IWeatherServiceExtended : IWeatherService
{
    /// <summary>
    /// Ottiene le previsioni meteo per un'area geografica rettangolare
    /// </summary>
    /// <param name="request">Richiesta con parametri dell'area e della previsione</param>
    /// <returns>Previsioni aggregate per l'area</returns>
    Task<WeatherAreaForecastDto> GetAreaForecastAsync(WeatherAreaRequest request);
    
    /// <summary>
    /// Ottiene dati meteorologici storici per una località
    /// </summary>
    /// <param name="location">Località o coordinate</param>
    /// <param name="startDate">Data di inizio</param>
    /// <param name="endDate">Data di fine</param>
    /// <returns>Dati storici per il periodo specificato</returns>
    Task<WeatherHistoryDto> GetHistoricalDataAsync(string location, DateTime startDate, DateTime endDate);
}
