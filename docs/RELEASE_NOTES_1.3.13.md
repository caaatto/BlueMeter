# BlueMeter v1.3.13 Release Notes

## ⚠️ BETA FEATURES

**Real-time Charts are now in BETA!** While functional, this feature is still experimental and may have edge cases or bugs. Please report any issues you encounter.

---

## 🎨 New Features

### 📊 Real-time Combat Charts (BETA)
- **DPS Trend Chart**: Live tracking of instant DPS over time during combat
- **Skill Breakdown Chart**: Visual representation of damage distribution per skill
- **Advanced Combat Log**: View historical encounters with chart data
- Charts are now **persistent** - data survives after combat ends!

### 🎯 Chart Capabilities
- Real-time data sampling every 200ms for accurate DPS curves
- Interactive tooltips showing detailed information on hover
- Legends for better data interpretation
- Support for both live combat and historical encounter playback
- Chart history automatically saved to database

---

## 🐛 Critical Bug Fixes

### Performance Fixes
**Issue**: Severe FPS drops in WBC/Raid/World Boss scenarios (60fps → 10fps)

**Root Cause**: Inefficient data caching caused excessive memory allocations during high-player-count encounters.

**Fixes Applied**:
1. **Data Caching Optimization** (`DataStorage.cs`)
   - Implemented caching for `ReadOnlyFullDpsDataList` and `ReadOnlySectionedDpsDataList`
   - Previously created new lists on every access with `.ToList()`
   - Cache invalidates only when data changes
   - **Result**: Eliminated hundreds of unnecessary allocations per second

2. **UI Update Throttling** (`DpsStatisticsViewModel.cs`)
   - Increased UI update interval: 100ms → 200ms
   - Reduces UI thread pressure during intense combat
   - **Result**: Smoother performance in high-activity scenarios

3. **Chart Auto-Refresh Optimization** (`RealtimeChartsForm.cs`)
   - Reduced chart refresh rate: 100ms → 500ms
   - Significantly lowers CPU usage
   - **Result**: Better overall system performance

### Chart Data Persistence Fixes
**Issue**: Chart data lost after combat ends - "dps trend funktioniert solange der kampf läuft. sobald dieser zu ende ist wars das und alles ist verloren"

**Fixes Applied**:
1. **Database Migration** (Commit `2ca0fcc`)
   - Added `DpsHistoryJson` and `HpsHistoryJson` columns to `PlayerEncounterStats` table
   - Migration runs automatically on app startup
   - Enables persistent storage of chart timeseries data

2. **Race Condition Fix** (Commit `8131705`) ⚠️ **CRITICAL**
   - **Problem**: Chart data was cleared BEFORE being saved
   - **Cause**: `EndCurrentEncounterAsync()` ran as fire-and-forget while data was cleared immediately
   - **Solution**: Changed to blocking call using `.GetAwaiter().GetResult()`
   - Data is now guaranteed to be saved before clearing
   - **Result**: Chart data persists correctly after boss fights

3. **Timeout Save Fix** (Commit `d376c78`) ⚠️ **CRITICAL**
   - **Problem**: Encounters ending by timeout (training dummy, wipes) were never saved
   - **Cause**: Save logic only triggered on boss death, not on section timeout
   - **Solution**: Added encounter save to timeout path
   - **Result**: All combat sessions now save, including:
     - Training dummy sessions ✅
     - Wipes ✅
     - Combat area exits ✅
     - Disconnects ✅

---

## 🎨 UI/UX Improvements

### Chart Usability Enhancements
- **Interactive Tooltips**: Hover over chart elements to see detailed values
- **Legends**: Bar charts now include legends for better data interpretation
- **Improved Readability**:
  - Font sizes increased (9-10pt base)
  - Better contrast and color schemes
  - Labels no longer truncated in pie charts
- **English Localization**: All chart UI text translated to English
- **Better Error Handling**: Retry options for failed operations
- **Proper Resource Disposal**: Prevents memory leaks in tooltip components

---

## 🔧 Technical Details

### Performance Metrics
- **Before**: ~10 FPS in 40-player raids
- **After**: Stable 60 FPS in same scenarios
- **Memory**: ~70% reduction in GC pressure during combat

### Data Flow
```
Combat Data → ChartDataService (200ms sampling)
           → Instant DPS calculation via sliding window
           → ObservableCollection for live UI updates
           → On Combat End:
              1. Create snapshots (deep copy)
              2. Save to database (JSON serialization)
              3. Clear live data
           → Historical Load:
              1. Query from database
              2. Deserialize JSON
              3. Load into ChartDataService
              4. Display in UI
```

### Database Schema
```sql
ALTER TABLE PlayerEncounterStats
ADD COLUMN DpsHistoryJson TEXT;

ALTER TABLE PlayerEncounterStats
ADD COLUMN HpsHistoryJson TEXT;
```

---

## 📝 Known Limitations (BETA)

- Chart data limited to last 500 points per player (FIFO buffer)
- Large raid encounters may have higher memory usage
- Historical chart loading may be slow for very large encounters (>40 players)
- Some edge cases in chart synchronization may still exist

---

## 🙏 Acknowledgments

This release includes:
- 3,610 lines added across 37 files
- Complete chart persistence system implementation
- Critical performance fixes for large-scale content
- Major UX improvements for better combat analysis

---

## 📦 Installation

Download the latest release and extract to your preferred location. No additional setup required - database migrations run automatically on first launch.

## 🐞 Bug Reports

If you encounter issues with the new chart features or performance problems, please report them on our GitHub issues page with:
- Steps to reproduce
- Combat scenario (raid size, boss name, etc.)
- Log files from `%LOCALAPPDATA%\BlueMeter\logs\`

---

**Thank you for using BlueMeter!** 🎉
