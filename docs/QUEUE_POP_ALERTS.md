# Queue Pop Alert System - Implementation Documentation

## Overview

The queue pop alert system has been fully implemented to notify players when a dungeon/PVP queue pops. The system uses two detection methods:

1. **Heuristic Detection** (Currently Active): Detects when 3+ players join within 2 seconds
2. **Protobuf-Based Detection** (Needs Method ID): Monitors `CharTeam.IsMatching` state changes

## Current Status

✅ **Fully Implemented & Working:**
- Sound player service with fallback to Windows system sounds
- Queue alert manager that subscribes to queue pop events
- Settings UI with sound selection, volume control, and test button
- Heuristic queue detection (3+ players in 2 seconds)
- Debug logging for queue detection

⚠️ **Requires Configuration:**
- Sound files (MP3s) need to be added to `BlueMeter.WPF/Assets/sounds/`
- CharTeam message Method ID needs to be found and added

## Features

### Settings (in Settings → Alerts)

1. **Enable Queue Pop Sound**: Toggle queue pop alerts on/off
2. **Alert Sound**: Choose from 4 sounds (Drum, Harp, Wow, Yoooo)
3. **Volume**: Adjust alert volume (0-100%)
4. **Test Sound**: Play a test sound with current settings
5. **Queue Detection Logging**: Enable debug logging to help find the CharTeam Method ID

### Sound Fallback System

If MP3 files are not found, the system automatically falls back to Windows system sounds:
- Drum → SystemSounds.Exclamation
- Harp → SystemSounds.Asterisk
- Wow → SystemSounds.Beep
- Yoooo → SystemSounds.Hand

## How to Use

### Basic Usage (Works Now)

1. Launch BlueMeter
2. Go to Settings → Alerts
3. Enable "Queue Pop Sound"
4. Select your preferred sound and volume
5. Click "Test Sound" to preview
6. The alert will play when 3+ players join within 2 seconds (heuristic detection)

### Adding Custom Sound Files (Optional)

1. Place MP3 files in: `BlueMeter.WPF/Assets/sounds/`
2. Required file names:
   - `drum.mp3`
   - `harp.mp3`
   - `wow.mp3`
   - `yoooo.mp3`
3. Recommended sources:
   - https://freesound.org/
   - https://mixkit.co/free-sound-effects/
   - https://pixabay.com/sound-effects/

## Advanced: Enabling Protobuf-Based Detection

The system has a `TeamMatchingProcessor` that can detect queue pops by monitoring the `CharTeam.IsMatching` protobuf field. However, we need to find the correct Method ID for CharTeam messages.

### Finding the CharTeam Method ID

1. **Enable Debug Logging:**
   - Go to Settings → Alerts
   - Enable "Queue Detection Logging (Debug)"

2. **Join a Queue:**
   - Launch the game while BlueMeter is running
   - Join a dungeon or PVP queue
   - Accept the queue when it pops

3. **Check the Logs:**
   - Look for lines like: `[UNKNOWN METHOD ID: 0xXXXXXXXX (NNNNNNNN)]`
   - These appear when the game sends messages that aren't being processed
   - The CharTeam messages will likely appear when you join/leave a queue

4. **Update the Code:**
   - Open: `BlueMeter.Core/Analyze/MessageMethod.cs`
   - Replace `TeamMatching = 0x00000000U` with the actual Method ID
   - Example: `TeamMatching = 0x00000042U` (if the ID is 0x42)

5. **Enable the Processor:**
   - Open: `BlueMeter.Core/Analyze/V2/Processors/MessageHandlerRegistry.cs`
   - Uncomment the line:
     ```csharp
     { MessageMethod.TeamMatching, new TeamMatchingProcessor(storage, logger) }
     ```

6. **Rebuild:**
   ```bash
   dotnet build BlueMeter.WPF/BlueMeter.WPF.csproj -c Release
   ```

### Benefits of Protobuf-Based Detection

- **More Accurate**: Detects the exact moment the queue pops
- **No False Positives**: Won't trigger when players randomly join
- **State-Based**: Monitors `IsMatching: true → false` transition

## Architecture

### Component Overview

```
DataStorageV2 (Core)
  ├─ QueuePopDetected Event
  ├─ TriggerQueuePopDetected() Method
  └─ Heuristic Detection (3+ players in 2s)

TeamMatchingProcessor (Core)
  ├─ Parses CharTeam protobuf messages
  ├─ Monitors IsMatching state changes
  └─ Triggers QueuePopDetected event

QueueAlertManager (WPF)
  ├─ Subscribes to QueuePopDetected event
  └─ Calls SoundPlayerService

SoundPlayerService (WPF)
  ├─ Plays MP3 files via MediaPlayer
  └─ Fallback to SystemSounds
```

### Code Locations

- **Settings UI**: `BlueMeter.WPF/Views/SettingsView.xaml` (line 524-650)
- **Settings ViewModel**: `BlueMeter.WPF/ViewModels/SettingsViewModel.cs`
- **Config**: `BlueMeter.WPF/Config/AppConfig.cs` (line 253-282)
- **Sound Service**: `BlueMeter.WPF/Services/SoundPlayerService.cs`
- **Alert Manager**: `BlueMeter.WPF/Services/QueueAlertManager.cs`
- **Data Storage**: `BlueMeter.Core/Data/DataStorageV2.cs` (line 44-53, 356-368)
- **Team Processor**: `BlueMeter.Core/Analyze/V2/Processors/TeamMatchingProcessor.cs`
- **Message Method**: `BlueMeter.Core/Analyze/MessageMethod.cs` (line 11-14)
- **Registry**: `BlueMeter.Core/Analyze/V2/Processors/MessageHandlerRegistry.cs` (line 21-22)

## Testing

### Test the Alert System

1. **Test Sound Playback:**
   - Go to Settings → Alerts
   - Click "Test Sound"
   - Verify sound plays (or system sound if no MP3)

2. **Test Heuristic Detection:**
   - Enable "Queue Detection Logging"
   - Join an instance with multiple players
   - Check logs for: `[QUEUE DETECTION] ⚠️ POTENTIAL QUEUE POP DETECTED!`
   - Verify sound plays

3. **Test Protobuf Detection (After Method ID is found):**
   - Enable "Queue Detection Logging"
   - Join a queue and wait for it to pop
   - Check logs for: `[TeamMatchingProcessor] 🎉 QUEUE POP DETECTED!`
   - Verify sound plays

## Troubleshooting

### Sound Not Playing

1. **Check Settings:**
   - Verify "Enable Queue Pop Sound" is ON
   - Volume is not 0%

2. **Check Sound Files:**
   - If using custom MP3s, verify files exist in `Assets/sounds/`
   - System sounds should work as fallback

3. **Check Logs:**
   - Look for errors like: `Failed to play sound: {Sound}`
   - Enable queue detection logging for more details

### False Positives (Heuristic)

- The heuristic method may trigger when many players load into an area
- This is expected behavior until protobuf-based detection is enabled
- To reduce false positives: Find and enable the CharTeam Method ID

### No Detection at All

1. **Verify DataStorageV2 is Running:**
   - Check that BlueMeter is capturing packets
   - Verify network adapter is selected in Settings

2. **Check Event Subscription:**
   - The QueueAlertManager should log: `QueueAlertManager initialized`
   - Check logs on startup

## Future Improvements

Potential enhancements for the queue pop alert system:

1. **Visual Notifications:**
   - Toast notifications via TaskbarIcon
   - Window flashing/bringing to front
   - Overlay notification

2. **Advanced Detection:**
   - Multiple detection methods running in parallel
   - Configurable sensitivity for heuristic detection
   - Ready check detection

3. **Sound Customization:**
   - Volume per sound type
   - Custom sound file upload via UI
   - Text-to-speech alerts

4. **Smart Filtering:**
   - Only alert for specific queue types
   - Cooldown period to prevent spam
   - Different sounds for different queue types

## Contributing

If you find the CharTeam Method ID or want to improve the detection, please:

1. Test the changes thoroughly
2. Document the Method ID you found
3. Submit a pull request with your findings

---

**Generated with Claude Code** 🤖
