# StarResonanceDpsToolBox TODO List

## TODO List

### TODO

- [ ] SettingsView -> Theme setting needs to be implemented.
 - [x] MessageView -> Consider refactoring to MVVM (if needed).
 - [x] SettingsView (VM) -> Review the i18n usage in Cancel() for correctness.
 - [x] 玩家信息更新
 - [x] 战斗信息不刷新
 - [ ] 治疗技能列表不正确
 - [x] Fix TryDetect Server when there is no process running
 - [x] RegisterHotKey failed for Topmost: F7+Control
 - [ ] simplify logging
 - [ ] logging detail for syncing
 - [ ] Optimize nuget references
 - [ ] i18n fallback
 - [ ] DPS Statistics View -> Binding error
 - [ ] Export DPS statistics image
 - [ ] Not to update port filter if the ports are same

### Issues

- [ ] Incorrect detection of the player's class
- [ ] (WPF) Refresh promptly after retrieving cached user information
- [ ] (WPF) Manual TopMost toggle sometimes fails to clear

### Features

- [ ] Local data caching feature
- [ ] (WinForm) Attempt GPU-accelerated control rendering
- [ ] Allow launching with Shift/Ctrl held to reset user settings

### Remaining issues to be examined

- [ ] 将团队总伤显示到DPS统计
- [ ] DPS统计增加滚动条
- [ ] 打桩模式增加木桩选择，可选最右侧或者NPC后面的那根，以免有能叠加减防等debuff给木桩或者有其他人在打同一木桩增伤导致自身伤害异常
- [ ] NPC数据增加
- [ ] 数据收集加上等级和臂章等级

### CheckList

- [ ] Synchronize window transparency with mouse-through; window should be transparent only when mouse-through is enabled
