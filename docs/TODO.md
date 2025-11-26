# BlueMeter TODO List

## TODO List

### TODO

- [x] SettingsView -> Theme setting implementation
- [x] Comprehensive theme system with color filters
- [x] Live theme preview in settings
- [x] Background image support with full opacity
- [x] Theme color overlay on background images
- [x] Semi-transparent UI panels for contrast
- [x] MessageView -> Consider refactoring to MVVM (if needed)
- [x] SettingsView (VM) -> Review the i18n usage in Cancel() for correctness
- [x] Player information update
- [x] Combat information not refreshing
- [ ] Healing skill list is incorrect
- [x] Fix TryDetect Server when there is no process running
- [x] RegisterHotKey failed for Topmost: F7+Control
- [x] Simplify logging
- [ ] Logging detail for syncing
- [ ] Optimize nuget references
- [ ] i18n fallback
- [ ] DPS Statistics View -> Binding error
- [ ] Export DPS statistics image
- [ ] Not to update port filter if the ports are same

### Issues

- [x] Incorrect detection of the player's class
- [ ] (WPF) Refresh promptly after retrieving cached user information
- [ ] (WPF) Manual TopMost toggle sometimes fails to clear
- [ ] Class, name, and specialization detection takes very long
- [ ] Hover popup flickers when clicking without moving mouse
- [x] Skill breakdown window stays frozen at opening state - needs to update live
- [x] Min/Max/Crit values showing 0 in hover popup during combat
- [ ] Translation incorrect - check if skill_mapping is being used

### Features - DPS Meter Core

- [ ] DPS Over Time Graph - Visualize how DPS changes throughout the battle
- [ ] Party/Team Comparison Chart - Show percentage of total damage per player
- [ ] Peak DPS / Burst Damage Tracking - Highest DPS spike in combat
- [ ] Average vs Instantaneous DPS - Distinguish between sustained and burst
- [ ] Damage Type Breakdown - Physical vs Magical vs Elemental damage split
- [ ] **Tanking/Mitigation Stats** - Track damage taken and mitigation efficiency
  - [ ] Parse `ShieldLessenValue` from network packets (damage absorbed/mitigated)
  - [ ] Extend `BattleLog` struct with `ShieldLessenValue` field
  - [ ] Update `DeltaInfoProcessors` to capture shield/mitigation data
  - [ ] Add `TotalDamageMitigated` to `DpsData` model
  - [ ] Update database schema to store mitigation data (with migration)
  - [ ] Display metrics in UI:
    - Damage Taken (current HP damage)
    - Damage Mitigated (shields/absorbs/blocks)
    - Effective Damage (total threat = taken + mitigated)
    - Mitigation % ((mitigated / effective) × 100)
  - [ ] Add "Effective TPS" (Threat Per Second) including shields
  - [ ] Show mitigation breakdown in skill breakdown window
  - **Priority: High** - Critical for tank performance analysis
- [ ] Healing Efficiency Metrics - Heals per second and overhealing tracking
- [ ] Combat Log Export - Save combat data as CSV/JSON for analysis
- [ ] Cooldown/Ability Tracking - Show which abilities are on cooldown
- [ ] Team Synergy Detection - Highlight beneficial ability combinations

### Features - General

- [ ] Local data caching feature
- [ ] (WinForm) Attempt GPU-accelerated control rendering
- [ ] Allow launching with Shift/Ctrl held to reset user settings

### Remaining issues to be examined

- [ ] Display team total damage in DPS statistics
- [ ] Add scrollbar to DPS statistics
- [ ] Add training dummy selection in dummy mode (select rightmost or NPC-behind dummy to avoid debuff stacking or damage interference from other players)
- [ ] Add NPC data
- [ ] Add level and will level to data collection