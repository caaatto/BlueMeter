# Daily/Weekly Checklist - Benutzerhandbuch

## Übersicht

Die Daily/Weekly Checklist hilft dir, deine täglichen und wöchentlichen Tasks in BlueMeter zu tracken.

## Features

### ✅ **Task-Verwaltung**
- **Daily Tasks**: Resetten täglich um 08:00 GMT+1
- **Weekly Tasks**: Resetten montags um 08:00 GMT+1
- **Incremental Progress**: Zähler für Tasks wie "3/9 Dungeon Runs"
- **Color-Coded Categories**:
  - Dungeon (Blau)
  - Raid (Lila)
  - PvP (Rot)
  - Crafting (Grün)
  - Social (Orange)
  - Dailies (Gelb)
  - Weeklies (Cyan)

### 🎮 **Bedienung**

#### **Maus**
- **Checkbox anklicken**: Task als erledigt markieren
- **Task-Item anklicken**: Toggle Completion (außer Checkbox/Buttons)
- **+ Button**: Erhöht Counter um 1
- **+ Button gedrückt halten**: Schnelles Inkrementieren (alle 100ms)
- **- Button**: Verringert Counter um 1
- **- Button gedrückt halten**: Schnelles Dekrementieren (alle 100ms)

#### **Tastatur**
- **Pfeiltasten ↑↓**: Navigation zwischen Tasks
- **Enter/Space**: Toggle Task Completion
- **Ctrl+F**: Focus auf Suchfeld

### ⏱️ **Event Timer**
Zeigt wichtige Event-Zeiten an:
- Daily Reset (08:00)
- Weekly Reset (Montag 08:00)
- World Boss Crusade (16:00 - 22:00)
- Guild Hunt (Fr-So 14:00 - 04:00)
- Guild Dance (Fr 15:30 - 03:30)

**Aktive Events** haben einen Glow-Effekt und zeigen "Active - Ends in"

### 📊 **Multi-Profil-Support**
- Erstelle mehrere Profile für verschiedene Charaktere
- Jedes Profil hat eigene Task-Listen und Fortschritt
- Wechsel zwischen Profilen über das Dropdown (oben rechts)

### 🔍 **Suche & Filter**
- **Suchfeld**: Filtert Tasks nach Label
- **Show Completed**: Toggle zum Ein-/Ausblenden erledigter Tasks

### 💾 **Import/Export**
- **Export**: Speichert aktuelles Profil als JSON-Datei
- **Import**: Lädt Profil aus JSON-Datei (neue Profile-ID wird generiert)
- **Auto-Save**: Änderungen werden automatisch gespeichert

### 🔄 **Batch-Aktionen**
- **Select All Daily/Weekly**: Markiert alle Tasks als erledigt
- **Deselect All Daily/Weekly**: Markiert alle Tasks als unerledigt

### ⚡ **Performance**
- **Window fokussiert**: Timer updaten jede Sekunde
- **Window unfokussiert**: Timer updaten alle 5 Sekunden (spart Ressourcen)

## Speicherort

Alle Daten werden gespeichert in:
```
%AppData%/BlueMeter/Checklist/
├── config.json                    # Konfiguration
├── profile_<ID>.json              # Profil-Daten
└── ...
```

## Standard-Tasks

### Daily Tasks (Beispiel)
- Complete 3 Dungeons (0/3)
- Daily PvP Matches (0/5)
- Crafting Daily Quest
- Guild Contribution

### Weekly Tasks (Beispiel)
- Weekly Raid Clear
- Bane Lord Defeats (0/9)
- Weekly PvP Participation

## Anpassung

### Neue Tasks hinzufügen (Code)
Bearbeite `ChecklistProfile.cs` in der `CreateDefault()` Methode:

```csharp
profile.DailyTasks.Add(new ChecklistTask
{
    Label = "Mein Custom Task",
    Category = TaskCategory.Dungeon, // oder Raid, PvP, etc.
    Type = TaskType.Daily,
    IsIncremental = true,  // false für einfache Checkbox
    CurrentProgress = 0,
    MaxProgress = 10       // z.B. 10 Runs
});
```

### Event-Schedule anpassen
Bearbeite `ChecklistConfig.cs` → `CreateDefault()`:

```csharp
config.ActiveEventSchedules.Add(new EventSchedule
{
    EventName = "Mein Event",
    StartTime = new TimeSpan(18, 0, 0),  // 18:00
    EndTime = new TimeSpan(20, 0, 0),     // 20:00
    ActiveDays = [DayOfWeek.Monday, DayOfWeek.Wednesday] // oder null für täglich
});
```

## Troubleshooting

**Tasks resetten nicht automatisch**
- Stelle sicher, dass die Anwendung läuft (mindestens einmal pro Tag starten)
- Checke Logs in `%AppData%/BlueMeter/logs/`

**Timer zeigen falsche Zeit**
- Zeitzone-Einstellung in `ChecklistConfig.cs` prüfen
- Standard: "W. Europe Standard Time" (GMT+1)

**Daten gehen verloren**
- Nutze Export-Funktion für Backups
- JSON-Dateien in `%AppData%/BlueMeter/Checklist/` manuell sichern

## Keyboard Shortcuts Übersicht

| Aktion | Shortcut |
|--------|----------|
| Focus Search | `Ctrl + F` |
| Navigate Tasks | `↑↓` |
| Toggle Task | `Enter` / `Space` |
| Export | *Button* |
| Import | *Button* |

## Technische Details

- **Framework**: WPF (Windows Presentation Foundation) .NET 8.0
- **MVVM Pattern**: CommunityToolkit.Mvvm
- **Persistierung**: JSON via Newtonsoft.Json
- **Timezone**: TimeZoneInfo mit "W. Europe Standard Time"
- **Auto-Reset**: Hintergrund-Service prüft alle 5 Minuten
