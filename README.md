# WeatherAPI Service

Servizio API per le previsioni meteo che utilizza WeatherAPI.com come provider di dati meteorologici.

## Funzionalità

- Previsioni meteo correnti per località specifiche
- Previsioni meteo per i prossimi giorni (fino a 14 giorni)
- Previsioni per aree geografiche definite tramite coordinate
- Dati storici meteo
- Calcolo dell'affidabilità delle previsioni

## Come iniziare

1. Clona il repository
2. Modifica il file `appsettings.json` inserendo la tua API key di WeatherAPI.com
3. Esegui il progetto con `dotnet run`
4. Accedi a Swagger per testare le API all'indirizzo `https://localhost:7233/swagger`

## Requisiti

- .NET 9.0
- Un account WeatherAPI.com (gratuito o a pagamento)

## Note

- Le previsioni per area geografica sono implementate tramite aggregazione di punti campionati
- L'affidabilità delle previsioni è calcolata in base a diversi fattori (distanza temporale, variabilità, ecc.)
- I dati storici potrebbero essere limitati in base al piano sottoscritto su WeatherAPI.com
