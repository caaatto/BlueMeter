# Charts Implementation Plan for BlueMeter

## Overview
This document outlines the complete implementation plan for adding real-time chart visualization to BlueMeter, inspired by StarResonanceDps's chart system.

---

## Table of Contents
1. [Analysis of StarResonanceDps Implementation](#analysis-of-starresonancedps)
2. [Technical Approach](#technical-approach)
3. [Library Selection](#library-selection)
4. [Architecture Design](#architecture-design)
5. [Data Models](#data-models)
6. [Service Layer](#service-layer)
7. [Implementation Phases](#implementation-phases)
8. [Time Estimates](#time-estimates)

---

## Analysis of StarResonanceDps

### Key Findings from StarResonanceDps Charts

#### 1. Real-Time Windowing System
**Location**: `StarResonanceDpsAnalysis.WinForm\Plugin\DamageStatistics\PlayerStat.cs`

```csharp
// Line 18: Uses 1-second sliding window for instant DPS
private List<(DateTime timestamp, ulong damage)> _realtimeWindow = new();

// Line 142: RealtimeValue property for instant DPS calculation
public ulong RealtimeValue { get; private set; }

// Line 240-265: UpdateRealtimeStats() method
private void UpdateRealtimeStats()
{
    var cutoff = DateTime.UtcNow.AddSeconds(-1);
    _realtimeWindow.RemoveAll(x => x.timestamp < cutoff);
    RealtimeValue = (ulong)_realtimeWindow.Sum(x => (long)x.damage);
}
```

**Key Insights:**
- Maintains separate 1-second sliding window for real-time stats
- Removes expired entries automatically
- Calculates instant DPS from window sum
- Independent from full session tracking

#### 2. Background Data Collection
**Location**: `StarResonanceDpsAnalysis.WinForm\Plugin\StatisticalChart\ChartVisualizationService.cs`

**Architecture:**
- **Background Timer**: 200ms intervals
- **Data Sampling**: Periodic snapshots of DPS/HPS values
- **Storage Strategy**:
  - Separate storage for Current vs Full Session
  - Max 500 data points per player (FIFO cleanup)
  - Time-series format: `List<(DateTime, double)>`

**Code Pattern:**
```csharp
// Pseudo-code representation
Timer _updateTimer = new Timer(200); // 200ms interval
Dictionary<long, List<(DateTime, double)>> _playerDpsHistory;

void OnTimerTick()
{
    foreach (var player in GetActivePlayers())
    {
        var currentDps = player.RealtimeValue;
        _playerDpsHistory[player.Uid].Add((DateTime.UtcNow, currentDps));

        // Cleanup old data (keep max 500 points)
        if (_playerDpsHistory[player.Uid].Count > 500)
            _playerDpsHistory[player.Uid].RemoveAt(0);
    }

    RefreshCharts();
}
```

#### 3. Chart Types Implemented

**StarResonanceDps has 5 chart types:**
1. **DPS Trend Chart** - Real-time line chart showing DPS over time
2. **Skill Breakdown Pie Chart** - Damage distribution by skill
3. **Team DPS Bar Chart** - Player comparison
4. **Multi-Dimension Scatter Chart** - DPS vs Crit Rate correlation
5. **Damage Type Stacked Chart** - Damage type breakdown

#### 4. Custom WinForms Rendering
- They use custom Graphics API rendering
- High-quality anti-aliasing
- Microsoft YaHei font for Chinese support
- 10-color palette for data series
- Dark/Light theme support

---

## Technical Approach

### Our Approach vs StarResonanceDps

| Aspect | StarResonanceDps | BlueMeter (Our Plan) |
|--------|------------------|----------------------|
| **UI Framework** | WinForms | WPF |
| **Chart Library** | Custom Graphics | LiveCharts2 |
| **Update Frequency** | 200ms | 200ms (same) |
| **Data Window** | 1 second | 1 second (same) |
| **Max History** | 500 points | 500 points (same) |
| **Theme Support** | Dark/Light | Dark/Light (same) |

### Why Different Library?
- **WPF Native**: LiveCharts2 is designed for WPF (better integration)
- **MVVM Support**: Built-in data binding
- **Less Code**: No need to implement custom rendering
- **Maintained**: Active development and community support

---

## Library Selection

### Option 1: LiveCharts2 ⭐ **RECOMMENDED**
**NuGet**: `LiveChartsCore.SkiaSharpView.WPF`

**Pros:**
- ✅ Native WPF support with MVVM
- ✅ Beautiful default styling
- ✅ Excellent performance (SkiaSharp backend)
- ✅ Real-time updates support
- ✅ Built-in animations
- ✅ Active development
- ✅ Great documentation

**Cons:**
- ❌ Larger dependency size (~8MB)
- ❌ Learning curve for API

**Code Example:**
```csharp
// Simple line chart
public ObservableCollection<ISeries> Series { get; set; } = new()
{
    new LineSeries<double>
    {
        Values = new ObservableCollection<double> { 2, 5, 4, 6, 8 },
        Fill = null,
        GeometrySize = 0
    }
};
```

### Option 2: ScottPlot
**NuGet**: `ScottPlot.WPF`

**Pros:**
- ✅ Very fast rendering
- ✅ Lightweight
- ✅ Simple API
- ✅ Good for scientific plots

**Cons:**
- ❌ Less WPF-native (uses WinForms host)
- ❌ Styling requires more work
- ❌ Less suitable for real-time streaming

**Decision: Use LiveCharts2** for better WPF integration and real-time support.

---

## Architecture Design

### Component Overview

```
BlueMeter.WPF
├── Services/
│   └── ChartDataService.cs          # Background data collection
├── ViewModels/
│   ├── ChartsWindowViewModel.cs     # Main charts window VM
│   ├── DpsTrendChartViewModel.cs    # DPS trend chart VM
│   └── SkillBreakdownViewModel.cs   # Skill breakdown VM
├── Views/
│   └── ChartsWindow.xaml            # Charts window UI
└── Models/
    └── ChartDataPoint.cs            # Time-series data point

BlueMeter.Core
└── Models/
    └── DpsData.cs                   # Extended with real-time window
```

### Data Flow

```
PacketAnalyzer
    ↓
DataStorage (damage events)
    ↓
DpsData (accumulates damage)
    ↓ [Every damage event]
DpsData.UpdateRealtimeStats() (sliding window)
    ↓ [Every 200ms]
ChartDataService.SampleData() (reads RealtimeValue)
    ↓
ChartDataService._history (stores time-series)
    ↓ [On UI update]
ChartsWindowViewModel (binds to LiveCharts)
    ↓
ChartsWindow.xaml (renders)
```

---

## Data Models

### 1. Extend DpsData with Real-Time Windowing

**File**: `BlueMeter.Core/Models/DpsData.cs`

```csharp
public partial class DpsData
{
    // Existing properties...
    public ulong TotalDamage { get; set; }
    public ulong TotalHeal { get; set; }

    // NEW: Real-time windowing
    private List<(DateTime timestamp, ulong damage)> _damageWindow = new();
    private List<(DateTime timestamp, ulong heal)> _healWindow = new();

    /// <summary>
    /// Instant DPS calculated from 1-second sliding window
    /// </summary>
    public ulong InstantDps { get; private set; }

    /// <summary>
    /// Instant HPS calculated from 1-second sliding window
    /// </summary>
    public ulong InstantHps { get; private set; }

    /// <summary>
    /// Add damage to sliding window and update instant DPS
    /// </summary>
    public void AddDamageToWindow(ulong damage)
    {
        _damageWindow.Add((DateTime.UtcNow, damage));
        UpdateRealtimeStats();
    }

    /// <summary>
    /// Add heal to sliding window and update instant HPS
    /// </summary>
    public void AddHealToWindow(ulong heal)
    {
        _healWindow.Add((DateTime.UtcNow, heal));
        UpdateRealtimeStats();
    }

    /// <summary>
    /// Update instant DPS/HPS from sliding windows
    /// </summary>
    private void UpdateRealtimeStats()
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-1);

        // Remove old entries
        _damageWindow.RemoveAll(x => x.timestamp < cutoff);
        _healWindow.RemoveAll(x => x.timestamp < cutoff);

        // Calculate instant values
        InstantDps = (ulong)_damageWindow.Sum(x => (long)x.damage);
        InstantHps = (ulong)_healWindow.Sum(x => (long)x.heal);
    }

    /// <summary>
    /// Clear sliding windows (on section end)
    /// </summary>
    public void ClearWindows()
    {
        _damageWindow.Clear();
        _healWindow.Clear();
        InstantDps = 0;
        InstantHps = 0;
    }
}
```

### 2. Chart Data Point Model

**File**: `BlueMeter.WPF/Models/ChartDataPoint.cs`

```csharp
namespace BlueMeter.WPF.Models;

/// <summary>
/// Represents a single data point in a time-series chart
/// </summary>
public record ChartDataPoint
{
    public DateTime Timestamp { get; init; }
    public double Value { get; init; }

    public ChartDataPoint(DateTime timestamp, double value)
    {
        Timestamp = timestamp;
        Value = value;
    }
}
```

---

## Service Layer

### ChartDataService

**File**: `BlueMeter.WPF/Services/ChartDataService.cs`

```csharp
using System.Collections.ObjectModel;
using System.Windows.Threading;
using BlueMeter.Core.Data;
using BlueMeter.WPF.Models;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.Services;

/// <summary>
/// Background service for collecting chart data at regular intervals
/// </summary>
public sealed class ChartDataService : IDisposable
{
    private readonly ILogger<ChartDataService> _logger;
    private readonly IDataStorage _dataStorage;
    private readonly DispatcherTimer _samplingTimer;

    // History storage: playerId -> time-series data
    private readonly Dictionary<long, ObservableCollection<ChartDataPoint>> _dpsHistory = new();
    private readonly Dictionary<long, ObservableCollection<ChartDataPoint>> _hpsHistory = new();

    private const int MaxHistoryPoints = 500;
    private const int SamplingIntervalMs = 200; // 200ms = 5 samples per second

    public ChartDataService(
        ILogger<ChartDataService> logger,
        IDataStorage dataStorage)
    {
        _logger = logger;
        _dataStorage = dataStorage;

        _samplingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(SamplingIntervalMs)
        };
        _samplingTimer.Tick += OnSamplingTick;

        // Subscribe to section events
        _dataStorage.NewSectionCreated += OnNewSectionCreated;
    }

    public void Start()
    {
        _samplingTimer.Start();
        _logger.LogInformation("Chart data service started (sampling interval: {Interval}ms)", SamplingIntervalMs);
    }

    public void Stop()
    {
        _samplingTimer.Stop();
        _logger.LogInformation("Chart data service stopped");
    }

    private void OnSamplingTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;

        // Sample DPS for all active players
        foreach (var player in _dataStorage.GetPlayersWithCombatData())
        {
            // Ensure history collection exists
            if (!_dpsHistory.ContainsKey(player.Uid))
            {
                _dpsHistory[player.Uid] = new ObservableCollection<ChartDataPoint>();
                _hpsHistory[player.Uid] = new ObservableCollection<ChartDataPoint>();
            }

            // Get instant DPS/HPS from sliding window
            var instantDps = player.InstantDps;
            var instantHps = player.InstantHps;

            // Add data points
            _dpsHistory[player.Uid].Add(new ChartDataPoint(now, instantDps));
            _hpsHistory[player.Uid].Add(new ChartDataPoint(now, instantHps));

            // Cleanup old data (FIFO)
            if (_dpsHistory[player.Uid].Count > MaxHistoryPoints)
                _dpsHistory[player.Uid].RemoveAt(0);
            if (_hpsHistory[player.Uid].Count > MaxHistoryPoints)
                _hpsHistory[player.Uid].RemoveAt(0);
        }
    }

    private void OnNewSectionCreated(object? sender, EventArgs e)
    {
        // Clear all history when new section starts
        _logger.LogInformation("New section created - clearing chart history");
        _dpsHistory.Clear();
        _hpsHistory.Clear();
    }

    /// <summary>
    /// Get DPS history for a specific player
    /// </summary>
    public ObservableCollection<ChartDataPoint>? GetDpsHistory(long playerId)
    {
        return _dpsHistory.TryGetValue(playerId, out var history) ? history : null;
    }

    /// <summary>
    /// Get HPS history for a specific player
    /// </summary>
    public ObservableCollection<ChartDataPoint>? GetHpsHistory(long playerId)
    {
        return _hpsHistory.TryGetValue(playerId, out var history) ? history : null;
    }

    public void Dispose()
    {
        _samplingTimer.Stop();
        _dataStorage.NewSectionCreated -= OnNewSectionCreated;
    }
}
```

---

## Implementation Phases

### Phase 1: Library Setup (1-2 hours)
**Goal**: Install and configure LiveCharts2

**Tasks:**
1. Add NuGet package `LiveChartsCore.SkiaSharpView.WPF` to BlueMeter.WPF
2. Test basic chart rendering with dummy data
3. Verify theme compatibility (dark/light mode)

**Deliverable**: Simple test window with a working line chart

---

### Phase 2A: Real-Time Windowing (2-3 hours)
**Goal**: Extend DpsData with sliding window logic

**Tasks:**
1. Add `_damageWindow` and `_healWindow` lists to DpsData
2. Implement `AddDamageToWindow()` and `AddHealToWindow()` methods
3. Implement `UpdateRealtimeStats()` method
4. Add `InstantDps` and `InstantHps` properties
5. Add `ClearWindows()` method

**Testing:**
- Verify sliding window removes old entries
- Verify instant DPS calculation is correct
- Verify windows clear on section end

**Deliverable**: DpsData with working real-time stats

---

### Phase 2B: Hook into DataStorage (1-2 hours)
**Goal**: Populate sliding windows from damage events

**Tasks:**
1. Find where damage is accumulated in DataStorage
2. Call `dpsData.AddDamageToWindow(damage)` on each damage event
3. Call `dpsData.AddHealToWindow(heal)` on each heal event
4. Call `dpsData.ClearWindows()` when section ends

**Files to Modify:**
- `BlueMeter.Core/Data/DataStorageV2.cs` (damage/heal accumulation)

**Testing:**
- Verify InstantDps updates during combat
- Verify windows clear between fights

**Deliverable**: Real-time stats populate during combat

---

### Phase 2C: ChartDataService (2-3 hours)
**Goal**: Background sampling service

**Tasks:**
1. Create `ChartDataService.cs`
2. Implement 200ms timer for sampling
3. Implement history storage (max 500 points)
4. Implement FIFO cleanup
5. Subscribe to NewSectionCreated event
6. Register service in DI container

**Files to Create:**
- `BlueMeter.WPF/Services/ChartDataService.cs`
- `BlueMeter.WPF/Services/IChartDataService.cs`

**Files to Modify:**
- `BlueMeter.WPF/Services/ApplicationStartup.cs` (register service)

**Testing:**
- Verify sampling occurs every 200ms
- Verify history stays under 500 points
- Verify history clears on section end

**Deliverable**: Background service collecting time-series data

---

### Phase 3: Charts Window UI (3-4 hours)
**Goal**: Create main charts window with tabs

**Tasks:**
1. Create `ChartsWindow.xaml` with TabControl
2. Create tabs for each chart type:
   - DPS Trend
   - Skill Breakdown
   - Player Comparison
3. Add window styling (theme support)
4. Add menu item in main window to open charts
5. Create `ChartsWindowViewModel.cs`

**Features:**
- Resizable window
- Remember window position/size
- Theme switching support
- Auto-refresh toggle
- Refresh button

**Deliverable**: Charts window UI with empty tabs

---

### Phase 4: DPS Trend Chart (3-4 hours)
**Goal**: Implement real-time DPS line chart

**Tasks:**
1. Create `DpsTrendChartViewModel.cs`
2. Bind to ChartDataService history
3. Configure LiveCharts LineSeries
4. Add player selection (show all or specific player)
5. Add time range slider (30s, 60s, 120s)
6. Implement chart theming

**Features:**
- Multi-player lines (different colors)
- Smooth interpolation
- Auto-scaling Y axis
- Time labels on X axis
- Legend with player names

**Deliverable**: Working DPS trend chart

---

### Phase 5: Skill Breakdown Chart (2-3 hours)
**Goal**: Pie chart showing skill damage distribution

**Tasks:**
1. Create `SkillBreakdownViewModel.cs`
2. Calculate skill damage percentages
3. Configure LiveCharts PieSeries
4. Add player dropdown selector
5. Show top 8 skills + "Others"

**Features:**
- Player selection dropdown
- Percentage labels
- Color-coded skills
- Hover tooltips

**Deliverable**: Working skill breakdown pie chart

---

### Phase 6: Player Comparison Chart (2-3 hours)
**Goal**: Bar chart comparing player DPS

**Tasks:**
1. Create `PlayerComparisonViewModel.cs`
2. Calculate average DPS per player
3. Configure LiveCharts ColumnSeries
4. Sort players by DPS (descending)
5. Add metric selector (DPS, Total Damage, HPS)

**Features:**
- Sorted bars
- Player names on X axis
- Value labels on bars
- Metric switcher

**Deliverable**: Working player comparison chart

---

### Phase 7: Theme & Polish (2-3 hours)
**Goal**: Theme support and UI polish

**Tasks:**
1. Implement dark/light theme switching
2. Match BlueMeter's color scheme
3. Add chart animations
4. Add loading states
5. Error handling (no data)
6. Performance optimization

**Deliverable**: Polished, themed charts

---

### Phase 8: Testing & Optimization (2-3 hours)
**Goal**: Final testing and performance tuning

**Tasks:**
1. Test with real combat data
2. Test theme switching
3. Test window resize/position saving
4. Memory leak testing
5. Performance profiling
6. Bug fixes

**Deliverable**: Production-ready charts feature

---

## Time Estimates

| Phase | Description | Estimated Time |
|-------|-------------|----------------|
| Phase 1 | Library Setup | 1-2 hours |
| Phase 2A | Real-Time Windowing | 2-3 hours |
| Phase 2B | Hook into DataStorage | 1-2 hours |
| Phase 2C | ChartDataService | 2-3 hours |
| Phase 3 | Charts Window UI | 3-4 hours |
| Phase 4 | DPS Trend Chart | 3-4 hours |
| Phase 5 | Skill Breakdown | 2-3 hours |
| Phase 6 | Player Comparison | 2-3 hours |
| Phase 7 | Theme & Polish | 2-3 hours |
| Phase 8 | Testing | 2-3 hours |
| **TOTAL** | | **20-29 hours** |

---

## Technical Decisions Summary

### ✅ Decisions Made

1. **Library**: LiveCharts2 (WPF native, MVVM support, real-time updates)
2. **Sampling Rate**: 200ms (5 samples/second)
3. **Window Size**: 1 second sliding window for instant stats
4. **History Limit**: 500 data points per player (FIFO)
5. **Chart Types**:
   - DPS Trend (line chart)
   - Skill Breakdown (pie chart)
   - Player Comparison (bar chart)
6. **Theme Support**: Dark/Light mode matching BlueMeter

### 🔄 Implementation Strategy

- **Incremental**: Build phase by phase
- **Test-Driven**: Test each phase before moving on
- **Data-First**: Build data layer before UI
- **Performance-Conscious**: Background sampling, limited history

---

## Reference Code from StarResonanceDps

### Real-Time Window Implementation
**File**: `StarResonanceDpsAnalysis.WinForm/Plugin/DamageStatistics/PlayerStat.cs`

```csharp
// Lines 240-265
private void UpdateRealtimeStats()
{
    var cutoff = DateTime.UtcNow.AddSeconds(-1);
    _realtimeWindow.RemoveAll(x => x.timestamp < cutoff);
    RealtimeValue = (ulong)_realtimeWindow.Sum(x => (long)x.damage);
}
```

### Background Sampling Pattern
**File**: `StarResonanceDpsAnalysis.WinForm/Plugin/StatisticalChart/ChartVisualizationService.cs`

```csharp
// Timer-based sampling
_autoRefreshTimer = new System.Windows.Forms.Timer
{
    Interval = 100, // 100ms high-frequency refresh
    Enabled = false
};
_autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

private void AutoRefreshTimer_Tick(object sender, EventArgs e)
{
    RefreshAllCharts();
}
```

---

## Next Steps

When ready to implement:

1. **Start with Phase 1**: Install LiveCharts2 and test basic rendering
2. **Proceed sequentially**: Each phase builds on the previous
3. **Test thoroughly**: Verify each component before moving on
4. **Ask questions**: Clarify any technical details as needed

---

**Last Updated**: 2025-01-18
**Status**: Planning Phase (Now)
**Priority**: Medium (after current bug fixes)
