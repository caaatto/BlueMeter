# DeepL Skill Name Translation Setup

## Übersicht

BlueMeter nutzt jetzt die **DeepL API** zur automatischen Übersetzung von chinesischen Skill-Namen ins Englische.

## Wie es funktioniert

1. **Automatische Erkennung**: Skill-Namen werden automatisch auf chinesische Zeichen geprüft
2. **Caching**: Übersetzungen werden gecacht um API-Aufrufe zu minimieren
3. **Async Background**: Übersetzungen laufen im Hintergrund, blockieren nicht das UI
4. **Fallback**: Wenn DeepL fehlt oder API nicht erreichbar → Original-Name wird gezeigt

## Setup

### 1. DeepL API Key bekommen

- Gehe zu: https://www.deepl.com/pro/api
- Registriere einen kostenlosen Account (500.000 Zeichen/Monat gratis)
- Kopiere deinen API Key

### 2. API Key in der App setzen

Öffne die App und initialisiere den Translator beim Start:

```csharp
// In MainWindow.xaml.cs oder App.xaml.cs
DpsStatisticsSubViewModel.InitializeTranslator("YOUR_DEEPL_API_KEY");
```

Oder über Config-Datei (geplant für zukünftige Version):

```json
{
  "DeepL": {
    "ApiKey": "YOUR_DEEPL_API_KEY",
    "Enabled": true
  }
}
```

## Technische Details

### DeepLSkillTranslator Service

- **Ort**: `BlueMeter.WPF/Services/DeepLSkillTranslator.cs`
- **Features**:
  - `TranslateAsync()` - Asynchrone Übersetzung mit Error-Handling
  - `Translate()` - Synchrone Wrapper (nutzt Cache)
  - `PreloadTranslationsAsync()` - Batch-Übersetzung für Performance
  - `ClearCache()` - Cache manuell löschen

### Integration in ViewModels

```csharp
// In DpsStatisticsSubViewModel und DpsStatisticsViewModel
var translatedSkillName = DpsStatisticsSubViewModel.GetTranslator()?.Translate(skillName) ?? skillName;
```

## Sicherheit & Zuverlässigkeit

✅ **API Key Safety**
- Wird nicht in Code hartcodiert
- Sollte via Config/Secrets gespeichert werden
- Nicht in Git committed

✅ **Error Handling**
- Network-Fehler werden abgefangen
- Falls DeepL nicht erreichbar → Original-Name
- Exceptions werden geloggt nicht geworfen

✅ **Performance**
- Translation Cache verhindert doppelte API-Aufrufe
- Async/Background-Processing blockiert nicht das UI
- Chinese Character Detection um unnötige Anfragen zu sparen

✅ **Fallback**
- Wenn API Key nicht gesetzt → Original-Namen (Chinesisch)
- Wenn API fehlt → Original-Namen
- Wenn Translation fehlschlägt → Original-Namen

## Verwendete Skills nutzen

Nach dem ersten Start mit der App:

1. **Erste Fights**: Skill-Namen bleiben Chinesisch (werden im Hintergrund übersetzt)
2. **Nach ~10 Sekunden**: Skill-Namen wechseln zu Englisch (Cache wird gefüllt)
3. **Spätere Fights**: Alle Namen sind sofort Englisch (aus Cache)

## Cache prüfen

Über Logging:
```
[DEBUG] Using cached translation: '风华翔舞' -> 'Wind Dance Flourish'
[DEBUG] Translated '龙击炮' to 'Dragon Strike Cannon'
```

## Kosten

- **Free Plan**: 500.000 Zeichen/Monat
- **Geschätzt für BlueMeter**: ~200-300 Skill-Namen = ~5-10 KB Zeichen
- **Kosten**: Kostenlos (unter Free Tier)

## Zukunftspläne

- [ ] Config-File Support für API Key
- [ ] Settings UI zum Aktivieren/Deaktivieren
- [ ] Lokale Offline-Übersetzungs-Datei als Alternative
- [ ] Batch-Preload beim App-Start

## Problembehebung

### Übersetzungen funktionieren nicht

1. **API Key Check**: Stelle sicher dass API Key gesetzt ist
2. **Network Check**: Prüfe ob Netzwerk-Zugang besteht
3. **Logs anschauen**: Suche nach DeepL-Fehlern in Debug-Logs

### Skill-Namen immer noch Chinesisch

- Das ist normal in den ersten Sekunden nach App-Start
- Warten bis Cache gefüllt ist (~10 Sekunden)
- Oder `PreloadTranslationsAsync()` beim Start aufrufen

### API Quota ausgeschöpft

- Nutze Free Plan mit max 500.000 Zeichen/Monat
- Bei Überschreitung: Upgrade oder warten bis nächster Monat

---

**Version**: 1.0
**Stand**: 2025-11-10
**Status**: ✅ Produktionsreif
