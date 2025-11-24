# Charts Implementation Plan for BlueMeter

## 🚨 CRITICAL ISSUE: Chart Data Persistence (2025-11-20)

### Problem
**Status**: 🟡 **IN TESTING** - Implementation complete, awaiting real-world test

**User Report**: "dps trend funktioniert solange der kampf läuft. sobald dieser zu ende ist wars das und alles ist verloren, denn die speicherfunktion von 'History' funktioniert nicht"

**Priority**: ⭐⭐⭐ **HIGHEST** - "Das speichern der Kämpfe hat oberste Prio"

### Root Cause Analysis

#### 1. Data Loss Flow
```
Fight Ends
    ↓
DataStorageV2.CheckSectionTimeout() [line 408-449]
    ↓
ShouldEndBattleSection() returns true (boss dead + 8s delay)
    ↓
EndCurrentEncounterAsync() - saves encounter to DB
    ↓
PrivateClearDpsData() - clears SectionedDpsData
    ↓
RaiseNewSectionCreated() event fires
    ↓
ChartDataService.OnNewSectionCreated() [line 124-139]
    ↓
_dpsHistory.Clear()  ❌ ALL CHART DATA LOST!
_hpsHistory.Clear()  ❌ ALL CHART DATA LOST!
```

#### 2. What Gets Saved vs What Gets Lost

**✅ Currently Saved to Database:**
- `PlayerInfo` (name, class, stats)
- `DpsData` (total damage, total healing, skill breakdown)
- Aggregate statistics (DPS, HPS, crit rate, etc.)

**❌ Currently LOST (Not Saved):**
- `ChartDataPoint` time-series data (DPS over time)
- `ChartDataPoint` time-series data (HPS over time)
- Complete chart history needed for visualization

#### 3. Missing Components

**Database Schema:**
- ❌ No `DpsHistoryJson` field in `PlayerStats` table
- ❌ No `HpsHistoryJson` field in `PlayerStats` table

**Persistence Logic:**
- ❌ `DataStorageExtensions.SaveCurrentEncounterAsync()` doesn't save chart data
- ❌ `EncounterRepository.SavePlayerStatsAsync()` doesn't store chart history
- ❌ `ChartDataService` clears data BEFORE it can be saved

**Loading Logic:**
- ❌ `EncounterService.LoadEncounterAsync()` doesn't load chart data
- ❌ `ChartDataService` has no method to load historical data
- ❌ No UI integration to display historical charts

### Solution Plan

#### Phase P1: Database Schema Extension ⏳
**Files to Modify:**
- `BlueMeter.Core/Data/Models/Database/EncounterEntity.cs`
- `BlueMeter.Core/Data/Database/EncounterRepository.cs`

**Changes:**
```csharp
// Add to PlayerStatsEntity
public string? DpsHistoryJson { get; set; }
public string? HpsHistoryJson { get; set; }
```

**Database Migration:** Add two TEXT/NVARCHAR columns to `PlayerStats` table

---

#### Phase P2: Save Chart Data Before Clearing ⏳
**Files to Modify:**
- `BlueMeter.WPF/Services/ChartDataService.cs`
- `BlueMeter.Core/Data/DataStorageExtensions.cs`
- `BlueMeter.Core/Data/Database/EncounterRepository.cs`

**Strategy 1: Save Before Clear (RECOMMENDED)**
```csharp
// ChartDataService.cs
private void OnNewSectionCreated()
{
    try
    {
        // Create snapshot of data BEFORE clearing
        var dpsSnapshot = _dpsHistory.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList() // Deep copy
        );
        var hpsSnapshot = _hpsHistory.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList() // Deep copy
        );

        // Pass to persistence layer (fire & forget)
        _ = Task.Run(async () =>
            await DataStorageExtensions.SaveChartHistoryAsync(
                dpsSnapshot, hpsSnapshot));
    }
    finally
    {
        // Now safe to clear
        _dpsHistory.Clear();
        _hpsHistory.Clear();
    }
}
```

**Strategy 2: Expose Chart Data Getter (ALTERNATIVE)**
```csharp
// IChartDataService.cs
Dictionary<long, List<ChartDataPoint>> GetDpsHistorySnapshot();
Dictionary<long, List<ChartDataPoint>> GetHpsHistorySnapshot();
```

**DataStorageExtensions Changes:**
```csharp
public static async Task SaveCurrentEncounterAsync(
    IChartDataService? chartDataService = null)
{
    // Get chart history from service
    var dpsHistory = chartDataService?.GetDpsHistorySnapshot()
        ?? new Dictionary<long, List<ChartDataPoint>>();
    var hpsHistory = chartDataService?.GetHpsHistorySnapshot()
        ?? new Dictionary<long, List<ChartDataPoint>>();

    // Pass to repository
    await _encounterService.SavePlayerStatsAsync(
        playerInfos, dpsData, dpsHistory, hpsHistory);
}
```

---

#### Phase P3: Database Persistence ⏳
**Files to Modify:**
- `BlueMeter.Core/Data/Database/EncounterService.cs`
- `BlueMeter.Core/Data/Database/EncounterRepository.cs`

**Changes:**
```csharp
// EncounterService.cs
public async Task SavePlayerStatsAsync(
    Dictionary<long, PlayerInfo> playerInfos,
    Dictionary<long, DpsData> dpsDataDict,
    Dictionary<long, List<ChartDataPoint>> dpsHistory,
    Dictionary<long, List<ChartDataPoint>> hpsHistory)
{
    foreach (var kvp in dpsDataDict)
    {
        var uid = kvp.Key;
        var dpsData = kvp.Value;

        // Get chart history for this player
        var playerDpsHistory = dpsHistory.GetValueOrDefault(uid, new List<ChartDataPoint>());
        var playerHpsHistory = hpsHistory.GetValueOrDefault(uid, new List<ChartDataPoint>());

        if (playerInfos.TryGetValue(uid, out var playerInfo))
        {
            await repository.SavePlayerStatsAsync(
                _currentEncounterId,
                playerInfo,
                dpsData,
                playerDpsHistory,
                playerHpsHistory);
        }
    }
}

// EncounterRepository.cs
public async Task SavePlayerStatsAsync(
    string encounterId,
    PlayerInfo playerInfo,
    DpsData dpsData,
    List<ChartDataPoint> dpsHistory,
    List<ChartDataPoint> hpsHistory)
{
    // Serialize to JSON
    string dpsHistoryJson = JsonConvert.SerializeObject(dpsHistory);
    string hpsHistoryJson = JsonConvert.SerializeObject(hpsHistory);

    // Save to database
    playerStatsEntity.DpsHistoryJson = dpsHistoryJson;
    playerStatsEntity.HpsHistoryJson = hpsHistoryJson;
}
```

---

#### Phase P4: Load Chart Data from History ⏳
**Files to Modify:**
- `BlueMeter.Core/Data/Database/EncounterService.cs`
- `BlueMeter.WPF/ViewModels/EncounterHistoryViewModel.cs`
- `BlueMeter.WPF/Services/ChartDataService.cs`

**Loading Logic:**
```csharp
// EncounterService.cs
public async Task<EncounterData?> LoadEncounterAsync(string encounterId)
{
    // ... existing code ...

    // NEW: Load chart history
    if (!string.IsNullOrEmpty(stats.DpsHistoryJson))
    {
        playerData.DpsHistory = JsonConvert.DeserializeObject<List<ChartDataPoint>>(
            stats.DpsHistoryJson);
    }
    if (!string.IsNullOrEmpty(stats.HpsHistoryJson))
    {
        playerData.HpsHistory = JsonConvert.DeserializeObject<List<ChartDataPoint>>(
            stats.HpsHistoryJson);
    }
}

// ChartDataService.cs
public void LoadHistoricalChartData(Dictionary<long, List<ChartDataPoint>> dpsHistory,
                                     Dictionary<long, List<ChartDataPoint>> hpsHistory)
{
    _dpsHistory.Clear();
    _hpsHistory.Clear();

    foreach (var kvp in dpsHistory)
    {
        _dpsHistory[kvp.Key] = new ObservableCollection<ChartDataPoint>(kvp.Value);
    }
    foreach (var kvp in hpsHistory)
    {
        _hpsHistory[kvp.Key] = new ObservableCollection<ChartDataPoint>(kvp.Value);
    }

    _logger.LogInformation("Loaded historical chart data: {DpsPlayers} DPS, {HpsPlayers} HPS",
        dpsHistory.Count, hpsHistory.Count);
}
```

---

#### Phase P5: UI Integration ⏳
**Files to Modify:**
- `BlueMeter.WPF/ViewModels/EncounterHistoryViewModel.cs`
- `BlueMeter.WPF/ViewModels/ChartsWindowViewModel.cs`

**Flow:**
```
User clicks "Load Encounter" in History
    ↓
EncounterHistoryViewModel.LoadSelectedEncounterAsync()
    ↓
DataStorageExtensions.LoadEncounterAsync(encounterId)
    ↓
Extract chart data from EncounterData
    ↓
ChartDataService.LoadHistoricalChartData(dpsHistory, hpsHistory)
    ↓
Open ChartsWindow → DPS Trend Chart displays historical data
```

---

### Implementation Checklist

#### Database Layer
- [x] Add `DpsHistoryJson` to `PlayerStatsEntity` ✅
- [x] Add `HpsHistoryJson` to `PlayerStatsEntity` ✅
- [x] Add columns to `PlayerStats` database table ✅
- [ ] Test database migration

#### Service Layer
- [x] Add `GetDpsHistorySnapshot()` to `IChartDataService` ✅
- [x] Add `GetHpsHistorySnapshot()` to `IChartDataService` ✅
- [x] Add `LoadHistoricalChartData()` to `IChartDataService` ✅
- [x] Update `DataStorageExtensions.SaveCurrentEncounterAsync()` signature ✅
- [x] Update `EncounterService.SavePlayerStatsAsync()` signature ✅
- [x] Update `EncounterRepository.SavePlayerStatsAsync()` to serialize JSON ✅

#### Loading Layer
- [x] Update `EncounterService.LoadEncounterAsync()` to deserialize chart data ✅
- [x] Add chart history fields to `PlayerEncounterData` ✅
- [x] Test JSON serialization/deserialization ✅

#### Integration Layer
- [x] Wire chart data from `ChartDataService` → Database save ✅
- [x] Wire chart data from Database → `ChartDataService` load ✅
- [x] Inject `ChartDataService` into `ApplicationStartup` ✅
- [x] Pass `ChartDataService` to `DataStorageExtensions.InitializeDatabaseAsync()` ✅
- [ ] Test save flow end-to-end
- [ ] Test load flow end-to-end

#### UI Layer
- [x] Update `EncounterHistoryViewModel.LoadSelectedEncounterAsync()` ✅
- [x] Add dependency injection for `IChartDataService` and `ILogger` to `EncounterHistoryViewModel` ✅
- [x] Fix `DpsStatisticsViewModel` to inject dependencies properly ✅
- [x] Fix `DpsStatisticsDesignTimeViewModel` to provide design-time stubs ✅
- [ ] Display message if no chart data available (old encounters)
- [ ] Test historical chart display

#### Testing
- [ ] Test: Fight → Data saved → Load from history → Chart displays
- [ ] Test: Multiple players saved correctly
- [ ] Test: Old encounters (no chart data) handle gracefully
- [ ] Test: Long fights (500+ points) saved correctly
- [ ] Test: Chart axes and scaling correct for historical data

---

### Technical Notes

**JSON Size Estimation:**
- 500 points × 2 doubles (timestamp, value) × 8 bytes ≈ 8KB per player
- 8 players × 8KB × 2 (DPS + HPS) ≈ 128KB per encounter
- 100 encounters × 128KB ≈ 12.8MB (acceptable)

**Serialization Format:**
```json
[
  { "Timestamp": "2025-11-20T12:34:56.123Z", "Value": 12345.67 },
  { "Timestamp": "2025-11-20T12:34:56.323Z", "Value": 12456.78 }
]
```

**Backwards Compatibility:**
- Old encounters without chart data: Fields will be NULL
- UI should display "No chart data available" for old encounters
- New encounters will have full chart history

---

## Overview
This document outlines the complete implementation plan for adding real-time chart visualization to BlueMeter, inspired by StarResonanceDps's chart system.

**Current Status**: ✅ **Phase 4 Complete - DPS Trend Chart Live!** (Progress: ~60%)
**Critical Issue**: 🔴 **Chart Data Persistence Required** (See above)

---

## Table of Contents
1. [Quick Status Summary](#quick-status-summary)
2. [Completed Phases](#completed-phases)
3. [Current Features](#current-features)
4. [Upcoming Phases](#upcoming-phases)
5. [Technical Architecture](#technical-architecture)
6. [Time Estimates](#time-estimates)

---

## Quick Status Summary

### ✅ What's Working Now

**Phase 1-4 Complete:**
- ✅ OxyPlot 2.2.0 integrated and tested
- ✅ Real-time windowing (1-second sliding window)
- ✅ Background data collection (200ms sampling)
- ✅ ChartDataService running on app startup
- ✅ ChartsWindow with dark theme UI
- ✅ **DPS Trend Chart** with multi-player lines
- ✅ **Player names** displayed in chart legend
- ✅ **Player focus feature** (thicker line for focused player)
- ✅ **Click on player** → Opens chart with player focused
- ✅ **Background data collection** (data available after fight ends)

### 🚀 Key Features

**Data Collection:**
- Runs in background even when ChartsWindow is closed
- Collects DPS/HPS every 200ms
- Maintains up to 500 data points per player (FIFO)
- Automatic cleanup on new section/fight

**Chart Interaction:**
- Real-time updates (500ms refresh)
- Auto-scaling axes
- Multi-player visualization (up to 8 colors)
- Player focus with 4px thick line
- Dark theme matching BlueMeter

**User Workflow:**
```
Fight → Data collected in background
     → Open "Advanced Combat Log"
     → See complete fight analytics!

OR

Click on player in DPS meter
     → Opens chart with that player focused
     → Thicker line highlights the player
```

---

## Completed Phases

### ✅ Phase 1: Library Setup (COMPLETED)
**Duration**: 1-2 hours
**Status**: ✅ Complete

**Tasks Completed:**
1. ✅ Added NuGet package `OxyPlot.Wpf` Version 2.2.0
2. ✅ Tested basic chart rendering with dummy data
3. ✅ Verified theme compatibility (dark mode)
4. ✅ Created ChartTestWindow for testing
5. ✅ Build verified successful

**Files Created:**
- `BlueMeter.WPF/Views/ChartTestWindow.xaml`
- `BlueMeter.WPF/Views/ChartTestWindow.xaml.cs`
- `BlueMeter.WPF/ViewModels/ChartTestViewModel.cs`

**Deliverable**: ✅ Working test window with OxyPlot line chart

---

### ✅ Phase 2A: Real-Time Windowing (COMPLETED)
**Duration**: 2-3 hours
**Status**: ✅ Complete

**Implementation:**
Extended `BlueMeter.Core/Data/Models/DpsData.cs` with:
- 1-second sliding windows for damage and healing
- `InstantDps` and `InstantHps` properties
- `AddDamageToWindow()` and `AddHealToWindow()` methods
- `UpdateRealtimeStats()` method
- `ClearWindows()` method

**Code Pattern:**
```csharp
private List<(DateTime timestamp, long damage)> _damageWindow = new();
private List<(DateTime timestamp, long heal)> _healWindow = new();

public long InstantDps { get; private set; }
public long InstantHps { get; private set; }

private void UpdateRealtimeStats()
{
    var cutoff = DateTime.UtcNow.AddSeconds(-1);
    _damageWindow.RemoveAll(x => x.timestamp < cutoff);
    _healWindow.RemoveAll(x => x.timestamp < cutoff);
    InstantDps = _damageWindow.Sum(x => x.damage);
    InstantHps = _healWindow.Sum(x => x.heal);
}
```

**Deliverable**: ✅ DpsData with working real-time stats

---

### ✅ Phase 2B: Hook into DataStorage (COMPLETED)
**Duration**: 1-2 hours
**Status**: ✅ Complete

**Implementation:**
Modified `BlueMeter.Core/Data/DataStorageV2.cs`:
- Added `AddDamageToWindow()` calls in damage event handler
- Added `AddHealToWindow()` calls in heal event handler
- Added `ClearWindows()` calls on section clear

**Integration Points:**
- Damage accumulation → `sectionedData.AddDamageToWindow(log.Value);`
- Heal accumulation → `sectionedData.AddHealToWindow(log.Value);`
- Section clear → `ClearWindows()` for all players

**Deliverable**: ✅ Real-time stats populate during combat

---

### ✅ Phase 2C: ChartDataService (COMPLETED)
**Duration**: 2-3 hours
**Status**: ✅ Complete

**Implementation:**
Created background sampling service:
- 200ms DispatcherTimer
- Dictionary storage for DPS/HPS history
- FIFO cleanup (max 500 points)
- Event subscription for section clearing

**Files Created:**
- `BlueMeter.WPF/Services/IChartDataService.cs`
- `BlueMeter.WPF/Services/ChartDataService.cs`
- `BlueMeter.WPF/Models/ChartDataPoint.cs`

**Service Registration:**
- Registered in `App.xaml.cs` as Singleton
- Started automatically on app startup
- Runs in background continuously

**Key Features:**
```csharp
// Sampling every 200ms
_samplingTimer.Interval = TimeSpan.FromMilliseconds(200);

// FIFO cleanup
if (_dpsHistory[playerId].Count > MaxHistoryPoints)
    _dpsHistory[playerId].RemoveAt(0);

// Auto-clear on new section
_dataStorage.NewSectionCreated += OnNewSectionCreated;
```

**Deliverable**: ✅ Background service collecting time-series data

---

### ✅ Phase 3: Charts Window UI (COMPLETED)
**Duration**: 3-4 hours
**Status**: ✅ Complete

**Implementation:**
Created main charts window with:
- Dark theme styling (#1E1E1E background)
- TabControl with 4 tabs
- Header with controls (Refresh, Auto-Refresh toggle)
- Footer status bar
- Window management integration

**Files Created:**
- `BlueMeter.WPF/Views/ChartsWindow.xaml`
- `BlueMeter.WPF/Views/ChartsWindow.xaml.cs`
- `BlueMeter.WPF/ViewModels/ChartsWindowViewModel.cs`

**Files Modified:**
- `BlueMeter.WPF/Services/IWindowManagementService.cs` (added ChartsWindow property)
- `BlueMeter.WPF/Services/WindowManagementService.cs` (window creation logic)
- `BlueMeter.WPF/App.xaml.cs` (manual registration due to "Window" suffix)

**Tabs:**
1. 📈 **DPS Trend** - Real-time DPS chart (Phase 4) ✅
2. 🎯 **Skill Breakdown** - Skill damage breakdown (Phase 6) 📋
3. 👥 **Player Comparison** - Player comparison chart (Phase 5) 📋
4. 🧪 **Test** - Debug/status information

**UI Features:**
- Custom dark theme styles
- Resizable window (1200x800 default, 800x600 minimum)
- CenterScreen startup position
- Window owner set to MainView

**Menu Integration:**
- Added "Advanced Combat Log" menu item in DpsStatisticsView
- Replaced old "Skill Diary" menu item
- Localization in 3 languages (EN, CN, PT-BR)

**Deliverable**: ✅ Charts window UI with working tabs

---

### ✅ Phase 4: DPS Trend Chart (COMPLETED)
**Duration**: 3-4 hours
**Status**: ✅ Complete

**Implementation:**
Created real-time DPS trend chart with OxyPlot:
- Multi-player line series (up to 8 colors)
- Auto-scaling axes
- Real-time updates (500ms)
- Player name display
- Player focus feature

**Files Created:**
- `BlueMeter.WPF/ViewModels/DpsTrendChartViewModel.cs`
- `BlueMeter.WPF/Views/DpsTrendChartView.xaml`
- `BlueMeter.WPF/Views/DpsTrendChartView.xaml.cs`

**Key Features:**
```csharp
// 8-color palette for players
private readonly List<OxyColor> _availableColors = new()
{
    OxyColor.FromRgb(0, 122, 204),  // Blue
    OxyColor.FromRgb(255, 99, 71),  // Red
    OxyColor.FromRgb(50, 205, 50),  // Green
    OxyColor.FromRgb(255, 165, 0),  // Orange
    OxyColor.FromRgb(147, 112, 219),// Purple
    OxyColor.FromRgb(255, 20, 147), // Pink
    OxyColor.FromRgb(64, 224, 208), // Turquoise
    OxyColor.FromRgb(255, 215, 0),  // Gold
};

// Player focus with thicker line
series.StrokeThickness = isFocused ? 4 : 2;

// Player name from IDataStorage
Title = playerInfo.Name ?? $"Player {playerId}";
```

**Chart Configuration:**
- X-Axis: Time in seconds (auto-scaling, minimum 60s)
- Y-Axis: DPS value (auto-scaling, no decimals)
- Dark theme colors
- Grid lines (major and minor)
- Legend (top-right, semi-transparent background)

**Player Focus Feature:**
- Click on player in DPS meter → Opens chart with player focused
- Focused player has 4px line (vs 2px for others)
- `SetFocusedPlayer(long? playerId)` method
- Immediate chart update when focus changes

**Modified Files:**
- `BlueMeter.WPF/ViewModels/ChartsWindowViewModel.cs` (added FocusedPlayerId property)
- `BlueMeter.WPF/Views/ChartsWindow.xaml.cs` (added SetFocusedPlayer method)
- `BlueMeter.WPF/ViewModels/DpsStatisticsViewModel.cs` (modified OpenPlayerSkillBreakdown)

**Behavior Changes:**
- **Old**: Click on player → Opens separate SkillBreakdownView window
- **New**: Click on player → Opens ChartsWindow with player focused in DPS Trend tab

**Deliverable**: ✅ Working DPS trend chart with player focus

---

## Current Features

### Data Layer
- ✅ Real-time windowing (1-second sliding window)
- ✅ Background sampling (200ms intervals)
- ✅ FIFO cleanup (500 point limit)
- ✅ Automatic section clearing
- ✅ ObservableCollection for WPF binding

### UI Layer
- ✅ Dark theme matching BlueMeter
- ✅ Multi-tab navigation
- ✅ Auto-refresh toggle
- ✅ Manual refresh button
- ✅ Player focus highlighting
- ✅ Real-time chart updates

### Integration
- ✅ Menu item in DPS Statistics
- ✅ Player click handler
- ✅ Window management integration
- ✅ DI container registration
- ✅ Service lifecycle management

---

## Upcoming Phases

### 📋 Phase 5: Player Comparison Chart (2-3 hours)
**Goal**: Bar/column chart comparing player metrics

**Planned Features:**
- Horizontal or vertical bar chart
- Sortable by multiple metrics:
  - Average DPS
  - Total Damage
  - Average HPS
  - Peak DPS
- Player name labels
- Color-coded bars
- Value labels on bars

**OxyPlot Components:**
- `BarSeries` or `ColumnSeries`
- `CategoryAxis` for player names
- `LinearAxis` for values

**UI Design:**
- Metric selector dropdown (DPS / Total Damage / HPS / Peak)
- Sort order toggle (ascending / descending)
- Auto-refresh with DPS trend

**Deliverable**: Working player comparison chart

**Implementation Plan:**
1. Create PlayerComparisonChartViewModel
2. Create PlayerComparisonChartView
3. Calculate metrics from DpsData
4. Configure OxyPlot BarSeries
5. Add to Player Comparison tab
6. Test with multiple players

---

### 📋 Phase 6: Skill Breakdown Chart (2-3 hours) ⭐ IMPORTANT
**Goal**: Visualize skill damage distribution for selected player

**Planned Features:**
- Skill damage breakdown by percentage
- Top 8 skills + "Others" category
- Player selector dropdown
- Skill names with icons (if available)
- Percentage labels
- Total damage display

**Chart Types (to decide):**
- Option A: Pie chart (good for % distribution)
- Option B: Horizontal bar chart (better readability)
- Option C: Both (toggle between views)

**OxyPlot Components:**
- `PieSeries` for pie chart
- `BarSeries` for horizontal bars
- Custom colors per skill

**UI Design:**
- Player selector dropdown
- Chart type toggle (pie / bar)
- Skill details table below chart
- Click on skill → Show detailed breakdown?

**Data Source:**
- `DpsData.SkillRecords` (dictionary of skill ID → damage)
- Skill name lookup from game data
- Percentage calculation: skill damage / total damage

**Deliverable**: Working skill breakdown chart

**Implementation Plan:**
1. Create SkillBreakdownChartViewModel
2. Create SkillBreakdownChartView
3. Aggregate skill data from DpsData
4. Implement skill name lookup
5. Configure OxyPlot chart
6. Add player selector
7. Integrate into Skill Breakdown tab
8. Test with real combat data

---

### 📋 Phase 7: Polish & Enhancement (2-3 hours)
**Goal**: UI polish and additional features

**Planned Tasks:**
1. **Export功能:**
   - Export chart as image (PNG)
   - Export data as CSV
   - Copy to clipboard

2. **Chart Enhancements:**
   - Zoom/pan support for DPS Trend
   - Time range selector (30s / 60s / 120s / All)
   - Crosshair tooltip (hover to see exact value)
   - Chart annotations (phase markers, death events)

3. **Theme Support:**
   - Light theme variant
   - Custom color schemes
   - User preference saving

4. **Performance:**
   - Optimize chart rendering
   - Lazy loading for large datasets
   - Virtual scrolling for player list

5. **UX Improvements:**
   - Loading indicators
   - Empty state messages
   - Error handling (no data)
   - Keyboard shortcuts

**Deliverable**: Polished, production-ready charts

---

### 📋 Phase 8: Testing & Optimization (2-3 hours)
**Goal**: Final testing and bug fixes

**Testing Checklist:**
1. **Functional Testing:**
   - [ ] Charts update in real-time during combat
   - [ ] Background data collection works when window closed
   - [ ] Player focus works correctly
   - [ ] All tabs display correctly
   - [ ] Refresh button works
   - [ ] Auto-refresh toggle works

2. **Data Validation:**
   - [ ] DPS values match DPS meter
   - [ ] Skill percentages add up to 100%
   - [ ] Player comparison is accurate
   - [ ] FIFO cleanup works (500 point limit)

3. **Performance Testing:**
   - [ ] No memory leaks during long sessions
   - [ ] Chart rendering is smooth (60 FPS)
   - [ ] Background sampling doesn't lag UI
   - [ ] Large player count (10+ players)

4. **Integration Testing:**
   - [ ] Window position/size saving
   - [ ] Theme switching
   - [ ] Menu navigation
   - [ ] Player click handler

5. **Edge Cases:**
   - [ ] No combat data (empty state)
   - [ ] Single player (no comparison)
   - [ ] Very short fights (<5 seconds)
   - [ ] Very long fights (>10 minutes)
   - [ ] Window closed/reopened mid-fight

6. **Bug Fixes:**
   - Fix any discovered issues
   - Performance optimization
   - Code cleanup
   - Documentation updates

**Deliverable**: Production-ready charts feature

---

## Technical Architecture

### Component Overview

```
BlueMeter.WPF
├── Services/
│   ├── IChartDataService.cs          ✅ Background data collection interface
│   └── ChartDataService.cs           ✅ 200ms sampling, 500pt FIFO
├── ViewModels/
│   ├── ChartsWindowViewModel.cs      ✅ Main window VM, player focus
│   ├── DpsTrendChartViewModel.cs     ✅ DPS trend chart VM
│   ├── SkillBreakdownChartViewModel.cs  📋 Phase 6
│   └── PlayerComparisonChartViewModel.cs 📋 Phase 5
├── Views/
│   ├── ChartsWindow.xaml             ✅ Main window with tabs
│   ├── DpsTrendChartView.xaml        ✅ DPS trend chart
│   ├── SkillBreakdownChartView.xaml  📋 Phase 6
│   └── PlayerComparisonChartView.xaml 📋 Phase 5
└── Models/
    └── ChartDataPoint.cs             ✅ Time-series data point

BlueMeter.Core
└── Data/
    └── Models/
        └── DpsData.cs                ✅ Extended with sliding windows
```

### Data Flow

```
PacketAnalyzer
    ↓
DataStorageV2 (damage/heal events)
    ↓
DpsData.AddDamageToWindow() / AddHealToWindow()
    ↓
DpsData.UpdateRealtimeStats() (sliding window)
    ↓ [InstantDps / InstantHps updated]

ChartDataService (200ms timer)
    ↓
Samples InstantDps/InstantHps
    ↓
Stores in ObservableCollection<ChartDataPoint>
    ↓ [FIFO cleanup at 500 points]

DpsTrendChartViewModel (500ms update timer)
    ↓
Reads from ChartDataService
    ↓
Updates OxyPlot PlotModel
    ↓
PlotView renders chart
```

### Player Focus Flow

```
User clicks player in DPS meter
    ↓
DpsStatisticsViewModel.OpenPlayerSkillBreakdown(player)
    ↓
ChartsWindow.SetFocusedPlayer(playerId)
    ↓
ChartsWindowViewModel.SetFocusedPlayer(playerId)
    ↓
DpsTrendChartViewModel.SetFocusedPlayer(playerId)
    ↓
UpdateChart() → Focused player gets 4px line
    ↓
PlotModel.InvalidatePlot(true) → Chart refreshes
```

---

## Library Selection

### ✅ OxyPlot 2.2.0 (SELECTED)

**Why OxyPlot?**
- ✅ **Stable**: Version 2.2.0 (production-ready, not prerelease)
- ✅ **WPF Native**: Excellent WPF integration
- ✅ **MVVM Support**: Built-in PlotModel data binding
- ✅ **Performance**: Fast rendering for real-time data
- ✅ **Simple API**: Easy to learn and use
- ✅ **Theme Support**: Easy dark/light customization
- ✅ **No Issues**: Clean build, no XAML compiler errors
- ✅ **Active**: Well-maintained, good documentation

**Basic Usage:**
```xml
<oxy:PlotView Model="{Binding PlotModel}" />
```

```csharp
var plotModel = new PlotModel
{
    Title = "DPS Trend",
    Background = OxyColor.FromRgb(30, 30, 30),
    TextColor = OxyColors.White
};

var lineSeries = new LineSeries
{
    Title = "Player DPS",
    Color = OxyColor.FromRgb(0, 122, 204),
    StrokeThickness = 2
};

plotModel.Series.Add(lineSeries);
plotModel.InvalidatePlot(true); // Refresh
```

---

## Time Estimates

| Phase | Description | Estimated | Actual | Status |
|-------|-------------|-----------|--------|--------|
| Phase 1 | Library Setup (OxyPlot) | 1-2h | ~2h | ✅ Complete |
| Phase 2A | Real-Time Windowing | 2-3h | ~2h | ✅ Complete |
| Phase 2B | Hook DataStorage | 1-2h | ~1h | ✅ Complete |
| Phase 2C | ChartDataService | 2-3h | ~2h | ✅ Complete |
| Phase 3 | Charts Window UI | 3-4h | ~3h | ✅ Complete |
| Phase 4 | DPS Trend Chart | 3-4h | ~4h | ✅ Complete |
| Phase 5 | Player Comparison | 2-3h | - | 📋 Next |
| Phase 6 | Skill Breakdown | 2-3h | - | 📋 Pending |
| Phase 7 | Polish & Enhancement | 2-3h | - | 📋 Pending |
| Phase 8 | Testing & Optimization | 2-3h | - | 📋 Pending |
| **TOTAL** | | **20-29h** | **~14h** | **~60% Done** |

**Progress**: 6/10 phases complete
**Time Spent**: ~14 hours
**Time Remaining**: ~6-15 hours

---

## Technical Decisions Summary

### ✅ Decisions Made

1. **Library**: OxyPlot 2.2.0 (stable, WPF native, MVVM)
2. **Sampling Rate**: 200ms (5 samples/second)
3. **Window Size**: 1-second sliding window
4. **History Limit**: 500 points per player (FIFO)
5. **Update Rate**: Charts refresh every 500ms
6. **Theme**: Dark theme (#1E1E1E background)
7. **Player Click**: Opens ChartsWindow (not separate SkillBreakdown window)
8. **Focus Feature**: 4px thick line for focused player

### 🎯 Design Principles

- **Background First**: Data collection runs even when window closed
- **Performance**: Limited history, efficient updates
- **User Experience**: Click player → See their chart
- **MVVM**: Clean separation of concerns
- **Dark Theme**: Consistent with BlueMeter styling

---

## Next Steps

### ⏳ Immediate Next Phase: Player Comparison (Phase 5)

**Tasks:**
1. Create PlayerComparisonChartViewModel
2. Create PlayerComparisonChartView
3. Implement metric calculation (Avg DPS, Total Damage, etc.)
4. Configure OxyPlot BarSeries
5. Add metric selector UI
6. Integrate into Player Comparison tab
7. Test with multiple players

**Priority**: Medium (Skill Breakdown is higher priority)

### ⭐ High Priority: Skill Breakdown (Phase 6)

**Why Important:**
- Core feature for understanding DPS composition
- Requested by user specifically
- Replaces old SkillBreakdownView functionality
- High user value

**Tasks:**
1. Create SkillBreakdownChartViewModel
2. Create SkillBreakdownChartView
3. Aggregate skill data from DpsData
4. Implement player selector
5. Configure chart (pie or bar)
6. Add to Skill Breakdown tab
7. Test with real skill data

---

## OxyPlot Quick Reference

### Dark Theme Template
```csharp
var plotModel = new PlotModel
{
    Background = OxyColor.FromRgb(30, 30, 30),      // #1E1E1E
    TextColor = OxyColors.White,
    TitleColor = OxyColors.White,
    PlotAreaBorderColor = OxyColor.FromRgb(63, 63, 70)
};

var axis = new LinearAxis
{
    TitleColor = OxyColors.White,
    TextColor = OxyColors.White,
    TicklineColor = OxyColor.FromRgb(63, 63, 70),
    MajorGridlineStyle = LineStyle.Solid,
    MajorGridlineColor = OxyColor.FromRgb(45, 45, 48),
    MinorGridlineStyle = LineStyle.Dot,
    MinorGridlineColor = OxyColor.FromRgb(40, 40, 43)
};
```

### Real-Time Update Pattern
```csharp
// In timer tick handler
private void OnUpdateTick(object? sender, EventArgs e)
{
    // Get new data
    var newDataPoints = GetLatestData();

    // Update series
    foreach (var series in PlotModel.Series.OfType<LineSeries>())
    {
        series.Points.Clear();
        series.Points.AddRange(newDataPoints);
    }

    // Refresh chart
    PlotModel.InvalidatePlot(true);
}
```

---

**Last Updated**: 2025-11-19
**Status**: Phase 4 Complete ✅ (~60% Done)
**Library**: OxyPlot 2.2.0
**Next**: Phase 5 (Player Comparison) or Phase 6 (Skill Breakdown)
**Priority**: High
