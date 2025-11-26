# Queue Pop Detection - Technical Analysis & Better Detection Plan

## Current Implementation Status

### ✅ What's Already Working

**Phase 1 (Sound Infrastructure)** - COMPLETE
- Sound files: `drum.mp3`, `harp.mp3`, `wow.mp3`, `yoooo.mp3` ✅
- `SoundPlayerService` implemented ✅
- Settings UI with Alerts tab ✅
- Volume control and test button ✅
- All configuration persisted to `config.json` ✅

**Phase 2-3 (Queue Detection)** - IMPLEMENTED
- `QueueAlertManager` service ✅
- Event-based architecture (`DataStorageV2.QueuePopDetected` event) ✅
- **Current Detection Method**: Return Message Burst Detection ✅
  - Tracks all Return messages (MessageType = 3)
  - When 5+ Return messages occur within 2 seconds → Queue pop detected
  - 30-second cooldown to prevent spam
  - File: `DataStorageV2.cs:67-93` (TrackReturnMessage method)

---

## Current Detection Method: Return Message Bursts

### How It Works

```csharp
// File: MessageAnalyzerV2.cs:304-346
private void ProcessReturnMsg(ByteReader packet, bool isZstdCompressed)
{
    var stubId = packet.ReadUInt32BE();
    var payload = packet.ReadRemaining();

    // Track Return messages for queue pop detection
    if (_storage is Data.DataStorageV2 dataStorageV2)
    {
        dataStorageV2.TrackReturnMessage();  // ← Every Return message tracked
    }
}

// File: DataStorageV2.cs:67-93
public void TrackReturnMessage()
{
    var now = DateTime.UtcNow;
    _recentReturnMessageTimes.Add(now);

    // Remove old messages outside 2-second window
    _recentReturnMessageTimes.RemoveAll(time => (now - time) > _returnBurstWindow);

    // Check if we have enough Return messages (5+) in the window
    if (_recentReturnMessageTimes.Count >= MinReturnMessagesForQueuePop)
    {
        // Check cooldown to prevent spam
        if ((now - _lastQueuePopAlertTime) > _queuePopCooldown)
        {
            logger.LogInformation("[RETURN BURST] Queue pop detected! {Count} Return messages...",
                _recentReturnMessageTimes.Count);

            _lastQueuePopAlertTime = now;
            QueuePopDetected?.Invoke();  // ← Alert triggered!

            _recentReturnMessageTimes.Clear();
        }
    }
}
```

### Message Flow

```
Game Client                   Game Server
    |                              |
    |  Queue Request (Call)        |
    |---------------------------->|
    |                              |
    |  Matchmaking in progress...  |
    |                              |
    |  MATCH FOUND!                |
    |                              |
    |  Return msg 1 (response)     |
    |<----------------------------|
    |  Return msg 2                |
    |<----------------------------|
    |  Return msg 3                |
    |<----------------------------|
    |  Return msg 4                |
    |<----------------------------|
    |  Return msg 5                | ← BURST DETECTED! Queue pop!
    |<----------------------------|
    |  Return msg 6                |
    |<----------------------------|
```

---

## Message Type Architecture

### Top-Level Message Types (MessageType enum)

```csharp
// File: MessageType.cs
public enum MessageType
{
    None = 0,
    Call = 1,       // Client → Server RPC calls
    Notify = 2,     // Server → Client notifications (combat data, game state)
    Return = 3,     // Server → Client RPC responses ← CURRENT DETECTION
    Echo = 4,       // Keepalive/heartbeat
    FrameUp = 5,    // Client → Server batched packets
    FrameDown = 6   // Server → Client batched packets
}
```

### Message Processing Pipeline

```
TCP Stream
    ↓
PacketAnalyzerV2 (reassembles TCP packets)
    ↓
MessageAnalyzerV2.Process()
    ↓
    ├─→ MessageType.Notify → ProcessNotifyMsg()
    │   ├─→ ServiceUuid = 0x0000000063335342 (combat service)
    │   │   └─→ MethodId (0x2B = TeamMatching, 0x2E = DeltaInfo, etc.)
    │   └─→ Other ServiceUuids (non-combat, logged to non-combat-services.log)
    │
    ├─→ MessageType.Return → ProcessReturnMsg()  ← CURRENT DETECTION
    │   └─→ TrackReturnMessage() → Burst detection
    │
    └─→ MessageType.FrameDown → ProcessFrameDown()
        └─→ Recursively process nested packets
```

### Known Message Methods (MessageMethod enum)

```csharp
// File: MessageMethod.cs
public enum MessageMethod : uint
{
    SyncNearEntities = 0x00000006U,      // Player/NPC spawning
    SyncContainerData = 0x00000015U,     // Inventory/container data
    SyncContainerDirtyData = 0x00000016U,// Container updates
    SyncToMeDeltaInfo = 0x0000002EU,     // Player stats/info (self)
    SyncNearDeltaInfo = 0x0000002DU,     // Nearby player stats
    TeamMatching = 0x0000002BU            // Team/matchmaking (Method ID 43)
}
```

---

## Debug Logging System

### Currently Active Logs (when EnableQueueDetectionLogging = true)

**File**: `DataStorageV2.cs:40`
```csharp
public static bool EnableQueueDetectionLogging { get; set; } = true;
```

### Log Files Generated

1. **`logs/all-messages.log`**
   - ALL MessageType packets (Notify, Return, FrameDown, etc.)
   - Format: `[HH:mm:ss.fff] MessageType: {type} (0x{hex}), Compressed: {bool}`
   - Logged in: `MessageAnalyzerV2.cs:56-67` (byte[] path) and `118-129` (span path)

2. **`logs/return-messages.log`**
   - All Return messages with StubId and payload
   - Format: `[HH:mm:ss.fff] Return StubId: 0x{stubId}, PayloadSize: {bytes}`
   - Includes hex dump of payload (if ≤256 bytes)
   - Logged in: `MessageAnalyzerV2.cs:319-337` and `370-388`

3. **`logs/all-notify-messages.log`**
   - All Notify messages (all services, including combat)
   - Format: `[HH:mm:ss.fff] ServiceUuid: 0x{uuid}, MethodId: 0x{method} ({decimal}), Combat: {bool}`
   - Logged in: `MessageAnalyzerV2.cs:171-183`

4. **`logs/non-combat-services.log`**
   - Only non-combat service Notify messages
   - Format: `[HH:mm:ss.fff] NON-COMBAT SERVICE - ServiceUuid: 0x{uuid}, MethodId: 0x{method}`
   - Logged in: `MessageAnalyzerV2.cs:192-199` and `229-240`
   - **This is where matchmaking service messages should appear!**

5. **`logs/charteam-messages.log`**
   - CharTeam messages (TeamMatching method, ID 0x2B)
   - Format: `[HH:mm:ss.fff] CharTeam - TeamId: {id}, LeaderId: {id}, TeamNum: {num}, IsMatching: {bool/NULL}, MemberCount: {count}, CharIds: [...]`
   - Logged in: `TeamMatchingProcessor.cs:44-66`
   - **Note**: IsMatching field is always NULL (game doesn't use it)

---

## Previous Detection Attempts (DISABLED)

### ❌ Method 1: Player Join Burst Detection
**Status**: Disabled (unreliable, too many false positives)

```csharp
// DataStorageV2.cs:416-433 (commented out)
// Tracked when 3+ players joined within 2 seconds
// Problem: Triggered during normal gameplay (new players joining zones, etc.)
```

### ❌ Method 2: CharTeam.IsMatching Field
**Status**: Disabled (field always NULL)

```csharp
// TeamMatchingProcessor.cs:23
// NOTE: CharTeam.IsMatching field is always NULL in this game
// Cannot be used for detection
```

### ❌ Method 3: Message Gap Detection
**Status**: Disabled (unreliable)

```csharp
// TeamMatchingProcessor.cs:22
// NOTE: Message gap detection disabled
// Timer removed - no longer using gap-based detection
```

---

## The Problem: Need Better Detection

### Current Issues with Return Message Burst Detection

1. **Generic Signal**: All Return messages are counted, regardless of purpose
   - Return messages happen for many reasons (inventory updates, UI interactions, etc.)
   - We're detecting a "burst pattern" but not identifying the SPECIFIC queue pop message

2. **No Context**: We don't know WHICH Return message triggered the burst
   - StubId and payload are logged but not analyzed
   - Could be false positives from other server responses

3. **Pattern-Based, Not Event-Based**: We're inferring queue pop from patterns
   - Would be better to identify THE specific packet that means "match found"

### What We're Looking For

The **ideal** queue pop detection would be:

```csharp
// Instead of this (current):
if (returnMessageBurstDetected) → Queue pop!

// We want this (specific packet):
if (packet.ServiceUuid == MATCHMAKING_SERVICE
    && packet.MethodId == MATCH_FOUND) → Queue pop!

// Or this (specific Return message):
if (returnMessage.StubId == QUEUE_POP_STUB_ID) → Queue pop!
```

---

## New Plan: Find THE Queue Pop Packet

### Investigation Strategy

We need to capture and analyze packets during a real queue pop event to identify the specific message.

### Step 1: Enable All Debug Logging

**Action**: Ensure logging is enabled before queueing

```csharp
// File: DataStorageV2.cs:40
public static bool EnableQueueDetectionLogging { get; set; } = true;  // ← Already enabled
```

**What Gets Logged**:
- ✅ All messages (`all-messages.log`)
- ✅ Return messages with StubId/payload (`return-messages.log`)
- ✅ Non-combat services (`non-combat-services.log`) ← **KEY LOG**
- ✅ CharTeam messages (`charteam-messages.log`)

### Step 2: Capture Queue Pop Event

**User Action**:
1. Launch BlueMeter
2. Start packet capture (connect to server)
3. Queue for a dungeon/raid
4. **NOTE THE EXACT TIME** when queue pops (ready check appears)
5. Accept the queue
6. Stop capture after loading into instance

**Expected Timeline**:
```
[19:05:30.123] In queue, waiting...
[19:05:45.456] ← QUEUE POP! (user notes this time)
[19:05:47.789] Accepted, loading...
```

### Step 3: Analyze Logs Around Queue Pop Time

**Primary Target**: `logs/non-combat-services.log`

Look for messages around the noted time:
```
[19:05:44.xxx] NON-COMBAT SERVICE - ServiceUuid: 0x????????????????, MethodId: 0x????????
[19:05:45.456] NON-COMBAT SERVICE - ServiceUuid: 0x????????????????, MethodId: 0x???????? ← THIS ONE?
[19:05:45.457] NON-COMBAT SERVICE - ServiceUuid: 0x????????????????, MethodId: 0x????????
```

**Secondary Target**: `logs/return-messages.log`

Look for Return messages with unique patterns:
```
[19:05:45.456] Return StubId: 0x12345678, PayloadSize: 42 bytes
  Payload: 01 02 03 04 ...  ← Analyze hex dump
```

### Step 4: Identify Unique Patterns

**What to Look For**:

1. **Unique ServiceUuid** in non-combat services
   - Likely a matchmaking/dungeon finder service
   - Will have a unique 64-bit UUID

2. **Unique MethodId** within that service
   - Method for "match found" or "ready check"
   - Will be a 32-bit integer

3. **Unique Return StubId**
   - The RPC call ID that triggered queue pop response
   - Will be a 32-bit integer

4. **Payload Patterns**
   - Specific byte sequences that indicate queue pop
   - Team member UUIDs, instance ID, etc.

### Step 5: Implement Specific Detection

Once we identify the pattern, implement targeted detection:

#### Option A: Non-Combat Service Message

```csharp
// File: MessageAnalyzerV2.cs
// In ProcessNotifyMsg(), BEFORE returning on non-combat services

if (serviceUuid == MATCHMAKING_SERVICE_UUID)  // e.g., 0x1234567890ABCDEF
{
    if (methodId == MATCH_FOUND_METHOD_ID)  // e.g., 0x000000AB
    {
        // Parse payload to confirm
        // Trigger queue pop event
        if (_storage is Data.DataStorageV2 dataStorageV2)
        {
            dataStorageV2.TriggerQueuePopDetected();
        }
    }
    return;  // Still skip combat service processing
}
```

#### Option B: Specific Return StubId

```csharp
// File: MessageAnalyzerV2.cs
// In ProcessReturnMsg()

var stubId = packet.ReadUInt32BE();

if (stubId == QUEUE_POP_STUB_ID)  // e.g., 0xABCD1234
{
    // This specific Return message = queue pop!
    if (_storage is Data.DataStorageV2 dataStorageV2)
    {
        dataStorageV2.TriggerQueuePopDetected();
    }
    return;
}

// Remove generic burst detection after confirming specific detection works
```

#### Option C: Payload Analysis

```csharp
// If the packet has a specific payload pattern
var payload = packet.ReadRemaining();

if (payload.Length == EXPECTED_LENGTH && payload[0] == MAGIC_BYTE)
{
    // Parse structured data
    var reader = new ByteReader(payload);
    var eventType = reader.ReadUInt32BE();

    if (eventType == QUEUE_POP_EVENT_TYPE)
    {
        // Confirmed queue pop!
        if (_storage is Data.DataStorageV2 dataStorageV2)
        {
            dataStorageV2.TriggerQueuePopDetected();
        }
    }
}
```

---

## Testing Plan

### Phase A: Log Capture
1. Enable debug logging
2. Queue for dungeon
3. Note exact queue pop time
4. Capture logs
5. Analyze patterns

### Phase B: Pattern Identification
1. Find unique ServiceUuid/MethodId or StubId
2. Verify pattern appears ONLY during queue pop
3. Check for false positives (test without queueing)

### Phase C: Implementation
1. Add new MessageMethod enum value if needed
2. Create specific processor or detection logic
3. Remove generic burst detection (or keep as fallback)
4. Test extensively

### Phase D: Validation
1. Test in different queue types (dungeon, raid, PvP)
2. Verify no false positives during normal gameplay
3. Confirm 100% detection rate for queue pops
4. Performance testing (no lag during combat)

---

## Expected Outcomes

### Best Case: Specific Matchmaking Service Message

```csharp
// New enum value
public enum MessageMethod : uint
{
    // ... existing methods ...
    MatchmakingQueuePop = 0x????????U,  // ← Discovered from logs
}

// New processor
internal sealed class MatchmakingProcessor : IMessageProcessor
{
    public void Process(byte[] payload)
    {
        // Parse matchmaking message
        // Trigger queue pop event
        _storage.TriggerQueuePopDetected();
    }
}
```

**Advantages**:
- ✅ 100% accurate (specific event)
- ✅ No false positives
- ✅ Clean, event-based detection
- ✅ Can extract additional info (instance name, team members, etc.)

### Good Case: Specific Return StubId

```csharp
// In ProcessReturnMsg()
private const uint QUEUE_POP_STUB_ID = 0x????????;  // ← Discovered from logs

if (stubId == QUEUE_POP_STUB_ID)
{
    _storage.TriggerQueuePopDetected();
}
```

**Advantages**:
- ✅ Very accurate
- ✅ Simple implementation
- ⚠️ StubId might vary by request (need to verify)

### Fallback: Improved Burst Detection

If no specific packet found, improve current burst detection:

```csharp
// Combine multiple signals
if (returnMessageBurst
    && recentPlayerJoins > 3
    && charTeamMessageReceived)
{
    // Higher confidence queue pop
    _storage.TriggerQueuePopDetected();
}
```

---

## Next Steps

### Immediate Actions

1. **Review Existing Logs** (if any)
   - Check `BlueMeter.WPF/bin/Release/net8.0-windows/logs/`
   - Look for previous queue pop captures

2. **Prepare for Capture**
   - Ensure logging is enabled
   - Clear old logs or note timestamp
   - Queue for dungeon and capture event

3. **Analyze Captured Data**
   - Focus on `non-combat-services.log` first
   - Cross-reference with `return-messages.log`
   - Look for timing correlations

4. **Identify Pattern**
   - Document discovered ServiceUuid/MethodId or StubId
   - Test pattern against multiple queue pop events
   - Verify no false positives

5. **Implement Specific Detection**
   - Add new detection logic
   - Remove or refine burst detection
   - Test thoroughly

---

## File Reference

### Core Detection Files

- **MessageAnalyzerV2.cs** (`BlueMeter.Core/Analyze/V2/MessageAnalyzerV2.cs`)
  - Processes all messages (Notify, Return, FrameDown)
  - Implements debug logging
  - Calls `TrackReturnMessage()` for current burst detection

- **DataStorageV2.cs** (`BlueMeter.Core/Data/DataStorageV2.cs`)
  - Lines 39-93: Queue pop detection logic
  - `TrackReturnMessage()`: Current burst detection
  - `QueuePopDetected` event: Alert trigger

- **MessageMethod.cs** (`BlueMeter.Core/Analyze/MessageMethod.cs`)
  - Enum of known message method IDs
  - Add new methods here when discovered

- **MessageHandlerRegistry.cs** (`BlueMeter.Core/Analyze/V2/Processors/MessageHandlerRegistry.cs`)
  - Maps MessageMethod → IMessageProcessor
  - Register new processors here

- **TeamMatchingProcessor.cs** (`BlueMeter.Core/Analyze/V2/Processors/TeamMatchingProcessor.cs`)
  - Processes TeamMatching messages (0x2B)
  - Logs CharTeam data
  - Reference for creating new processors

### Alert System Files

- **QueueAlertManager.cs** (`BlueMeter.WPF/Services/QueueAlertManager.cs`)
  - Subscribes to `QueuePopDetected` event
  - Triggers sound playback
  - Already functional, no changes needed

- **SoundPlayerService.cs** (`BlueMeter.WPF/Services/SoundPlayerService.cs`)
  - Plays alert sounds
  - Already functional, no changes needed

---

## Summary

### Current State
- ✅ Alert system fully implemented and working
- ✅ Return message burst detection working (5+ messages in 2s)
- ⚠️ Detection is pattern-based, not event-based

### Goal
- 🎯 Find THE specific packet that indicates "match found"
- 🎯 Implement event-based detection instead of pattern-based
- 🎯 Achieve 100% accuracy with zero false positives

### Approach
1. Capture logs during real queue pop event
2. Analyze logs to identify unique ServiceUuid/MethodId or StubId
3. Implement specific detection for that packet
4. Replace/augment generic burst detection

### Success Criteria
- ✅ Alert triggers immediately on queue pop
- ✅ Zero false positives during normal gameplay
- ✅ Works across all queue types (dungeon, raid, PvP)
- ✅ Detection based on specific packet, not heuristics

---

**Status**: Ready to Capture & Analyze
**Next Action**: Queue for dungeon and capture logs around queue pop event
