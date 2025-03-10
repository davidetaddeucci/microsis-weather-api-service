# Microsis Weather API Service

Servizio API per le previsioni meteo che utilizza WeatherAPI.com come provider di dati meteorologici, sviluppato in C# .NET 9.0.

## Funzionalità

- **Previsioni meteo correnti** per località specifiche
- **Previsioni meteo future** per i prossimi giorni (fino a 14 giorni)
- **Previsioni per aree geografiche** definite tramite coordinate
- **Dati storici meteo** con sistema di fallback intelligente
- **Dati astronomici** (alba, tramonto, fasi lunari, ecc.)
- **Calcolo dell'affidabilità** delle previsioni
- **Ricerca di località** tramite testo

## Struttura del progetto

- `Models/` - Modelli di dati e DTO
- `Services/` - Interfacce e implementazioni dei servizi
- `Controllers/` - Controller API

## Prerequisiti

- .NET 9.0
- Un account WeatherAPI.com (gratuito o a pagamento)

## Installazione

1. Clona il repository:
   ```bash
   git clone https://github.com/davidetaddeucci/microsis-weather-api-service.git
   ```

2. Modifica il file `appsettings.json` inserendo la tua API key di WeatherAPI.com:
   ```json
   "WeatherApi": {
     "ApiKey": "TUA_CHIAVE_API_QUI"
   }
   ```

3. Esegui il progetto:
   ```bash
   dotnet run
   ```

4. Accedi a Swagger per testare le API:
   ```
   https://localhost:7233/swagger
   ```

## Endpoint principali

### Previsioni base

- `GET /api/weather/current?location={località}` - Previsioni meteo correnti
- `GET /api/weather/forecast?location={località}&days={giorni}` - Previsioni meteo per i prossimi giorni
- `GET /api/weather/locations?query={testo}` - Ricerca località

### Funzionalità avanzate

- `GET /api/weatherextended/area-forecast` - Previsioni meteo per un'area geografica
- `POST /api/weatherextended/area-forecast` - Previsioni meteo per un'area geografica (post)
- `GET /api/weatherextended/history` - Dati storici meteorologici

## Funzionalità in dettaglio

### Previsioni per area geografica

Permette di ottenere previsioni meteo aggregate per un'area rettangolare definita da due coordinate (angolo superiore destro e angolo inferiore sinistro). Il sistema:

- Campiona più punti all'interno dell'area
- Calcola media di temperature, precipitazioni e altre variabili
- Identifica le condizioni meteorologiche prevalenti
- Calcola l'indice di variabilità delle condizioni nell'area

### Dati storici meteo

Implementa un sistema intelligente che:

- Per date recenti (ultimi 7 giorni): Utilizza l'API standard di previsione
- Per date più vecchie: Tenta di utilizzare l'API storica (potrebbe richiedere un piano a pagamento)

### Dati astronomici

Include informazioni dettagliate sul sole e la luna:

- Orari di alba e tramonto
- Orari di sorgere e tramontare della luna
- Fase lunare
- Illuminazione lunare
- Indicatori di visibilità del sole e della luna

## Note

- L'accesso ai dati storici oltre i 7 giorni potrebbe richiedere un piano a pagamento su WeatherAPI.com
- Le previsioni oltre i 14 giorni non sono supportate
- Il servizio include resilienza HTTP con policy di retry per errori transitori

## Licenza

Proprietaria. Tutti i diritti riservati.